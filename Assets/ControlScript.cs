using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class ControlScript : MonoBehaviour
{
    private GameObject _cube;
    private MLInputController _controller;
    private const float _rotationSpeed = 30.0f;
    private const float _distance = 2.0f;
    private const float _moveSpeed = 1.2f;
    private bool _enabled = false;
    private bool _bumper = false;
    private Color _color;
    private Vector3 _scale;


    void Awake()
    {
        _cube = GameObject.Find("Cube");
        _cube.SetActive(true);
        _color = _cube.GetComponent<MeshRenderer>().material.color;
        _scale = _cube.transform.localScale;

        _enabled = true;
        MLInput.Start();
        MLInput.OnControllerButtonDown += OnButtonDown;
        MLInput.OnControllerButtonUp += OnButtonUp;
        _controller = MLInput.GetController(MLInput.Hand.Left);
        
    }

    private void Start()
    {
        StartCoroutine(MotorRunning());
    }

    void OnDestroy()
    {
        MLInput.OnControllerButtonDown -= OnButtonDown;
        MLInput.OnControllerButtonUp -= OnButtonUp;
        MLInput.Stop();
    }

    IEnumerator MotorRunning()
    {
        while (true)
        {
            if (_controller.TriggerValue > 0.1f && _enabled)
            {
                float wait_time = 1f;
                float force = _controller.TriggerValue;

                if (force > 0.1f && force < 0.3f)
                {
                    wait_time = 1.2f;
                }
                else if (force >= 0.3f && force < 0.8f)
                {
                    wait_time = 0.6f;
                }
                else if (force >= 0.8f)
                {
                    wait_time = 0.3f;
                }

                _controller.StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.Bump, MLInputControllerFeedbackIntensity.High);

                yield return new WaitForSeconds(wait_time);
            }

            yield return null;
        }
    }

    void Update()
    {
        //if (_bumper && _enabled)
        //{
        //    _cube.transform.Rotate(Vector3.up, +_rotationSpeed * Time.deltaTime);
        //}
        //CheckControl();

        if (_bumper && _enabled)
        {
            _cube.transform.position = _controller.Position;
            _cube.transform.rotation = _controller.Orientation;
        }
        else if(_controller.TriggerValue > 0.1f && _enabled)
        {
            _bumper = false;
            _cube.transform.Rotate(Vector3.up, -_rotationSpeed * Time.deltaTime * _controller.TriggerValue * 10f);
        }

        if (_controller.Touch1PosAndForce.z > 0.0f && _enabled)
        {
            float g = (_controller.Touch1PosAndForce.x + 1) / 2.0f;
            float b = (_controller.Touch1PosAndForce.y + 1) / 2.0f;
            Color color = new Color(1, g, b);
            _cube.GetComponent<MeshRenderer>().material.color = color;

            _cube.transform.localScale = _scale * (_controller.Touch1PosAndForce.z + 1);
        }
        
    }

    void CheckControl()
    {
        if (_controller.TriggerValue > 0.2f && _enabled)
        {
            _bumper = false;
            _cube.transform.Rotate(Vector3.up, -_rotationSpeed * Time.deltaTime * _controller.TriggerValue * 10f);
        }
        else if (_controller.Touch1PosAndForce.z > 0.0f && _enabled)
        {
            float X = _controller.Touch1PosAndForce.x;
            float Y = _controller.Touch1PosAndForce.y;
            float Z = _controller.Touch1PosAndForce.z;
            Vector3 forward = Vector3.Normalize(Vector3.ProjectOnPlane(transform.forward, Vector3.up));
            Vector3 right = Vector3.Normalize(Vector3.ProjectOnPlane(transform.right, Vector3.up));
            Vector3 force = Vector3.Normalize((X * right) + (Y * forward));
            _cube.transform.position += force * Time.deltaTime * _moveSpeed;
        }
    }

    void OnButtonDown(byte controller_id, MLInputControllerButton button)
    {
        if ((button == MLInputControllerButton.Bumper && _enabled))
        {
            _bumper = true;
            _controller.StartFeedbackPatternVibe(MLInputControllerFeedbackPatternVibe.ForceDown, MLInputControllerFeedbackIntensity.High);
        }
    }

    void OnButtonUp(byte controller_id, MLInputControllerButton button)
    {
        if ((button == MLInputControllerButton.Bumper && _enabled))
        {
            _bumper = false;
        }

        if (button == MLInputControllerButton.HomeTap)
        {
            _cube.SetActive(true);
            _cube.transform.position = transform.position + transform.forward * _distance;
            _cube.transform.rotation = transform.rotation;
            _enabled = true;
            _cube.GetComponent<MeshRenderer>().material.color = _color;
            _cube.transform.localScale = _scale;
        }
    }

}
