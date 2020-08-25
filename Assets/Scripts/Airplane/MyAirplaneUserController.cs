using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MyAirplaneController))]
    public class MyAirplaneUserController : MonoBehaviour {

        public bool pcInput = false;

        private MyAirplaneController m_Airplane; // the airplane controller we want to use


        private void Awake()
        {
        // get the airplane controller
        m_Airplane = GetComponent<MyAirplaneController>();
        }


        private void FixedUpdate()
        {
            // pass the input to the airplane!
            float collective_height = SimpleInput.GetAxis("L_Vertical");
            float pedals_rotation = SimpleInput.GetAxis("L_Horizontal"); 
            float cyclic_forward = SimpleInput.GetAxis("R_Vertical"); 
            float cyclic_sideway = SimpleInput.GetAxis("R_Horizontal");
            float throttle = 100.0f;

            if (pcInput)
            {
                collective_height = Input.GetAxis("AirplaneHeight");
                cyclic_forward = Input.GetAxis("Vertical");
                pedals_rotation = Input.GetAxis("AirplaneRota");
                cyclic_sideway = Input.GetAxis("Horizontal");
            }

            collective_height = Mathf.InverseLerp(-1f, 1f, collective_height);
            pedals_rotation = Mathf.InverseLerp(-1f, 1f, pedals_rotation);

            //Round values 
            collective_height = (Mathf.Floor(collective_height * 100f)) / 100f;
            pedals_rotation = (Mathf.Floor(pedals_rotation * 100f)) / 100f;
            cyclic_forward = (Mathf.Floor(cyclic_forward * 100f)) / 100f;
            cyclic_sideway = (Mathf.Floor(cyclic_sideway * 100f)) / 100f;
        

#if !MOBILE_INPUT
            float handbrake = CrossPlatformInputManager.GetAxis("Jump");
            m_Airplane.Move(h, v, v);
#else
            m_Airplane.Move(collective_height, pedals_rotation, cyclic_forward, cyclic_sideway, throttle);
#endif
        }
    }
