using System.Collections.Generic;
using UnityEngine;
using System;

namespace MyCarController
{
    public class CarController : MonoBehaviour
    {
        public enum Car
        {
            Front,
            Rear
        }

        [Serializable]
        public struct CarMove
        {
            public GameObject carModel;
            public WheelCollider wheelCollider;
            public GameObject WheelEffectObj;
            public Car car;
        }

        [Header("Car Settings")]
        public static float maxAcceleration = 30.0f;
        public static float breakAcceleration = 50.0f;
        public float turnSensitivity = 1.0f;
        public float maxSteerAngle = 30.0f;

        [Header("Friction Settings")]
        public float baseSideFriction = 2.5f; // Friction latérale de base (augmentée)
        public float baseForwardFriction = 2.0f; // Friction longitudinale de base
        public float speedFrictionFactor = 0.3f; // Réduction de friction en fonction de la vitesse
        public Vector3 _centerOfMass;

        [Header("Drift Settings")]
        public float driftFrictionMultiplier = 0.7f;
        public float driftStabilityFactor = 0.95f;
        public float driftForce = 5.0f;

        [Header("Car Components")]
        public List<CarMove> carMoves;
        private float moveInput;
        private float steerInput;
        bool isBraking;
        bool isDrifting;
        float driftTime;
        private Rigidbody carRb;

        void Start()
        {
            carRb = GetComponent<Rigidbody>();
            carRb.centerOfMass = _centerOfMass;
            ConfigureWheelFriction();
        }

        void Update()
        {
            GetInputs();
            AnimateWheels();
            WheelEffects();
        }

        void FixedUpdate()
        {
            Move();
            Steer();
            Brake();
            AdjustWheelFriction();
            ApplyDrift();
        }

        void GetInputs()
        {
            moveInput = Mathf.Clamp(Input.GetAxis("Vertical"), -1f, 1f);
            steerInput = Mathf.Clamp(Input.GetAxis("Horizontal"), -1f, 1f);
            isDrifting = Input.GetKey(KeyCode.LeftShift);

            // Ajouter une zone morte pour éviter les légers inputs
            if (Mathf.Abs(moveInput) < 0.1f) moveInput = 0f;
            if (Mathf.Abs(steerInput) < 0.1f) steerInput = 0f;
        }

        void Move()
        {
            foreach (var wheel in carMoves)
            {
                float targetTorque = moveInput * 600 * maxAcceleration * Time.fixedDeltaTime;
                wheel.wheelCollider.motorTorque = Mathf.Lerp(wheel.wheelCollider.motorTorque, targetTorque, Time.fixedDeltaTime * 5.0f);
            }
        }

        void Steer()
        {
            foreach (var wheel in carMoves)
            {
                if (wheel.car == Car.Front)
                {
                    float speedFactor = Mathf.Clamp01(carRb.linearVelocity.magnitude / 50.0f); // Ajuste selon la vitesse max
                    float dynamicSteerAngle = Mathf.Lerp(maxSteerAngle, maxSteerAngle * 0.2f, speedFactor); // Réduit l'angle max à haute vitesse
                    float dynamicSteerSensitivity = Mathf.Lerp(turnSensitivity, turnSensitivity * 0.5f, speedFactor); // Réduit la sensibilité à haute vitesse

                    float targetSteerAngle = steerInput * dynamicSteerSensitivity * dynamicSteerAngle;
                    wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, targetSteerAngle, Time.fixedDeltaTime * 10.0f);
                }
            }
        }

        void Brake()
        {
            isBraking = Input.GetKey(KeyCode.Space);
            foreach (var wheel in carMoves)
            {
                if (isBraking)
                {
                    wheel.wheelCollider.brakeTorque = 300 * breakAcceleration * Time.fixedDeltaTime;
                    wheel.wheelCollider.motorTorque = 0; // Empêche l'accélération lors du freinage
                }
                else
                {
                    wheel.wheelCollider.brakeTorque = 0;
                }
            }
        }

        void AnimateWheels()
        {
            foreach (var wheel in carMoves)
            {
                Quaternion rot;
                Vector3 pos;
                wheel.wheelCollider.GetWorldPose(out pos, out rot);
                wheel.carModel.transform.position = pos;
                wheel.carModel.transform.rotation = rot;
            }
        }

       void ApplyDrift()
       {
            // Suivi du temps passé en drift
            if (isDrifting)
                driftTime += Time.deltaTime; // Incrémenter si le drift est actif
            else
                driftTime = 0; // Réinitialiser si le drift est terminé
            
            foreach (var wheel in carMoves)
            {
                WheelFrictionCurve sideFriction = wheel.wheelCollider.sidewaysFriction;
                WheelFrictionCurve forwardFriction = wheel.wheelCollider.forwardFriction;
            
                if (isDrifting)
                {
                    // Roues arrière : réduction de la friction pour favoriser le drift
                    if (wheel.car == Car.Rear)
                    {
                        sideFriction.stiffness = Mathf.Lerp(sideFriction.stiffness, 0.6f, Time.deltaTime * 6.0f); // Réduction importante de la friction latérale
                        forwardFriction.stiffness = Mathf.Lerp(forwardFriction.stiffness, 0.9f, Time.deltaTime * 6.0f); // Réduction légère de la friction longitudinale
                    }
                    else
                    {
                        // Roues avant : légère réduction pour maintenir le contrôle
                        sideFriction.stiffness = Mathf.Lerp(sideFriction.stiffness, 1.1f, Time.deltaTime * 6.0f);
                        forwardFriction.stiffness = Mathf.Lerp(forwardFriction.stiffness, 1.2f, Time.deltaTime * 6.0f);
                    }   
                
                    // Perte progressive de vitesse longitudinale en fonction de la durée du drift
                    Vector3 velocity = carRb.linearVelocity;
                
                    // Calcul de la vitesse longitudinale (avant/arrière) et latérale (côté)
                    Vector3 forwardVelocity = transform.forward * Vector3.Dot(velocity, transform.forward); // Composante avant/arrière
                    Vector3 sidewaysVelocity = transform.right * Vector3.Dot(velocity, transform.right);   // Composante latérale
                
                    // Réduction progressive de la vitesse longitudinale
                    float driftSpeedReductionFactor = Mathf.Lerp(1.0f, 0.9f, driftTime / 20.0f); // Réduction maximale de 30% après 3 secondes de drift
                    forwardVelocity *= driftSpeedReductionFactor;
                
                    // Réassembler la vitesse avec une perte longitudinale uniquement
                    carRb.linearVelocity = forwardVelocity + sidewaysVelocity;
                
                    // Assistance latérale pour maintenir le drift
                    Vector3 driftForceDirection = transform.right * steerInput * (driftForce * 1.8f);
                    carRb.AddForce(driftForceDirection, ForceMode.Acceleration);
                
                    // Légère rotation de la voiture pour rendre le drift plus fluide
                    float driftAssist = 6.0f * steerInput * Time.fixedDeltaTime;
                    transform.Rotate(0, driftAssist, 0);
                }
                else
                {
                    // Conduite normale : réinitialiser les frictions
                    sideFriction.stiffness = Mathf.Lerp(sideFriction.stiffness, wheel.car == Car.Rear ? 1.2f : 1.5f, Time.deltaTime * 6.0f);
                    forwardFriction.stiffness = Mathf.Lerp(forwardFriction.stiffness, wheel.car == Car.Rear ? 1.6f : 1.8f, Time.deltaTime * 6.0f);
                }
                // Appliquer les nouvelles frictions
                wheel.wheelCollider.sidewaysFriction = sideFriction;
                wheel.wheelCollider.forwardFriction = forwardFriction;
            }
        }


        void AdjustWheelFriction()
        {
            float speed = carRb.linearVelocity.magnitude;
            float frictionAdjustment = Mathf.Clamp01(1 - (speed / 50.0f)) * speedFrictionFactor;

            foreach (var wheel in carMoves)
            {
                // Ajuster la friction latérale (sideways)
                WheelFrictionCurve sideFriction = wheel.wheelCollider.sidewaysFriction;
                sideFriction.stiffness = baseSideFriction + frictionAdjustment;
                wheel.wheelCollider.sidewaysFriction = sideFriction;

                // Ajuster la friction longitudinale (forward)
                WheelFrictionCurve forwardFriction = wheel.wheelCollider.forwardFriction;
                forwardFriction.stiffness = baseForwardFriction + frictionAdjustment;
                wheel.wheelCollider.forwardFriction = forwardFriction;
            }
        }

        void ConfigureWheelFriction()
        {
            foreach (var wheel in carMoves)
            {
                // Configurer la friction initiale
                WheelFrictionCurve sideFriction = wheel.wheelCollider.sidewaysFriction;
                sideFriction.stiffness = baseSideFriction;
                wheel.wheelCollider.sidewaysFriction = sideFriction;

                WheelFrictionCurve forwardFriction = wheel.wheelCollider.forwardFriction;
                forwardFriction.stiffness = baseForwardFriction;
                wheel.wheelCollider.forwardFriction = forwardFriction;
            }
        }

        void WheelEffects()
        {
            foreach(var wheel in carMoves)
            {
                if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift))
                {
                    wheel.WheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
                }
                else
                {
                    wheel.WheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
                }
            }
        }
    }
}