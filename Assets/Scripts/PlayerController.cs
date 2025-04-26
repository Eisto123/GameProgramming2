using System.Numerics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class PlayerController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;
		public Animator _animator;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;
		public GameObject character;
		public float SlopeChangeThreshold = 1;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		[Header("Fly")]
		public float FlySpeed = 6;
		public float flyGroundRadius = 4f;
		public bool _isFly = false;
		public float flyCD = 5;
		private float flyCDTimer = 5;

		[Header("Movement Bounds")]
		public UnityEngine.Vector3 BoundCenter = UnityEngine.Vector3.zero;
		public UnityEngine.Vector3 flyBoundSize = new UnityEngine.Vector3(240f, 20f, 240f);
		public UnityEngine.Vector3 walkBoundSize = new UnityEngine.Vector3(250f, 100f, 250f);

		// cinemachine
		private float _cinemachineTargetPitch;
		private float _cinemachineTargetYaw;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;
		private UnityEngine.Vector3 _groundNormal;
		private UnityEngine.Vector3 _lastGroundNormal;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

	
#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			FlyCheck();

			if (!_isFly)
			{
				flyCDTimer -= Time.deltaTime;
				Mathf.Clamp(flyCDTimer, 0, flyCD);

				JumpAndGravity();
				GroundedCheck();
				Move();
				ClampPositionInBounds(walkBoundSize);
			}
			else
			{
				//Debug.Log("is fly");
				Fly();
				ClampPositionInBounds(flyBoundSize);
				FlyGroundCheck();
			}
		}

		private void LateUpdate()
		{
			//CameraRotationFly();
			if (!_isFly)
			{
				//AlignToGround();
				CameraRotation();
			}
			else
				CameraRotationFly();
		}

		private void FlyCheck()
		{
			if (_input.fly && !_isFly && flyCDTimer<=0)
			{
				//Debug.Log("fly");
				flyCDTimer = flyCD;
				_isFly = true;
				_verticalVelocity = 0f;
				//transform.position = new UnityEngine.Vector3(transform.position.x, 50, transform.position.z);
				_controller.Move(new UnityEngine.Vector3(0, 15, 0));
				_animator.SetBool("fly",true);
				_animator.SetBool("walk",false);
				_animator.SetBool("idle",false);
			}
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			UnityEngine.Vector3 spherePosition = new UnityEngine.Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void AlignToGround()
		{
			if (Grounded)
			{
				UnityEngine.Quaternion slopeRotation = UnityEngine.Quaternion.FromToRotation(transform.up, _groundNormal) * transform.rotation;
				transform.rotation = slopeRotation;
			}
		}

		private void FlyGroundCheck()
		{
			// set sphere position, with offset
			UnityEngine.Vector3 spherePosition = new UnityEngine.Vector3(transform.position.x, transform.position.y+1, transform.position.z);
			bool ifGrounded = Physics.CheckSphere(spherePosition, flyGroundRadius, GroundLayers, QueryTriggerInteraction.Ignore);
			if (ifGrounded)
			{
				_isFly = false;
				// Reset to ground
				Physics.Raycast(transform.position+new UnityEngine.Vector3(0,1,0), transform.forward, out RaycastHit hitInfo, flyGroundRadius, GroundLayers);
				_controller.Move(hitInfo.point);
				transform.rotation = UnityEngine.Quaternion.Euler(0, _cinemachineTargetYaw, 0.0f);
				_cinemachineTargetPitch = 0;

				_animator.SetBool("fly",false);
				_animator.SetBool("walk",false);
				_animator.SetBool("idle",true);
			}
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;
				_cinemachineTargetYaw += _rotationVelocity;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = UnityEngine.Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(UnityEngine.Vector3.up * _rotationVelocity);
			}
		}

		private void CameraRotationFly()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				float pitchInput = _input.look.y * RotationSpeed * deltaTimeMultiplier;
        		float yawInput = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch += pitchInput;
        		_cinemachineTargetYaw += yawInput;
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				//CinemachineCameraTarget.transform.localRotation = UnityEngine.Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				//transform.Rotate(UnityEngine.Vector3.up * _rotationVelocity);
				transform.rotation = UnityEngine.Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: UnityEngine.Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == UnityEngine.Vector2.zero) 
			{
				targetSpeed = 0.0f;
			}

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new UnityEngine.Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			UnityEngine.Vector3 inputDirection = new UnityEngine.Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: UnityEngine.Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != UnityEngine.Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
				_animator.SetBool("walk", true);
				_animator.SetBool("idle", false);
			}
			else
			{
				_animator.SetBool("idle", true);
				_animator.SetBool("walk", false);
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new UnityEngine.Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}

		private void Fly()
		{
			float targetSpeed = FlySpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: UnityEngine.Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == UnityEngine.Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new UnityEngine.Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			UnityEngine.Vector3 inputDirection = new UnityEngine.Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: UnityEngine.Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != UnityEngine.Vector2.zero)
			{
				// move, only fly forward, no left and right
				inputDirection = transform.forward * Mathf.Clamp(_input.move.y,0,1);
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime));
		
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void ClampPositionInBounds(UnityEngine.Vector3 boundSize)
		{
			Bounds bounds = new Bounds(BoundCenter, boundSize);

			UnityEngine.Vector3 clampedPosition = transform.position;
			clampedPosition.x = Mathf.Clamp(clampedPosition.x, bounds.min.x, bounds.max.x);
			clampedPosition.y = Mathf.Clamp(clampedPosition.y, bounds.min.y, bounds.max.y);
			clampedPosition.z = Mathf.Clamp(clampedPosition.z, bounds.min.z, bounds.max.z);

			transform.position = clampedPosition;
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new UnityEngine.Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);

		}

        private void OnDrawGizmos()
        {
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
			Gizmos.color = transparentRed;
            Gizmos.DrawSphere(transform.position+new UnityEngine.Vector3(0,1,0), flyGroundRadius);

			//Gizmos.DrawCube(BoundCenter, flyBoundSize);

        }

    }
}