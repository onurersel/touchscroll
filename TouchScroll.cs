using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class TouchScroll : MonoBehaviour {


	//enums
	private enum InputState { Down, Up}
	public enum Axis { XAndY, XOnly, YOnly }

	//private variables
	private Vector3 m_velocity;
	private Camera r_camera;
	private bool m_isEnabled;


	// Inspector Options
	[Range(0.001f,1f)]
	public float drag = 0.06f;
	public Axis axis = Axis.XAndY;
	public bool autoEnable = true;
	


	// Returns if mouse or touch is down/up, depending on platform support for touchscreen
	private InputState m_inputState
	{
		get
		{
#if (UNITY_ANDROID  ||  UNITY_IOS  ||  UNITY_WP8  ||  UNITY_WP8_1  ||  UNITY_BLACKBERRY)  &&  ! UNITY_EDITOR
			if(Input.touchCount > 0)
				return InputState.Down;
			else
				return InputState.Up;
			
#else
			if(Input.GetMouseButton(0))
				return InputState.Down;
			else
				return InputState.Up;
#endif
		}
	}

	// Returns touch or mouses position on world space
	private Vector2 m_cursorWorldPosition
	{
		get
		{
#if (UNITY_ANDROID  ||  UNITY_IOS  ||  UNITY_WP8  ||  UNITY_WP8_1  ||  UNITY_BLACKBERRY)  &&  ! UNITY_EDITOR
			if (Input.touchCount > 0)
				return r_camera.ScreenToWorldPoint(Input.GetTouch(0).position);
			else
				return r_camera.ScreenToWorldPoint(Vector2.zero);

#else
			return r_camera.ScreenToWorldPoint(Input.mousePosition);
#endif
		}		
	}




	// Initting on awake
	void Awake()
	{
		r_camera = GetComponent<Camera>();
		
		if(! r_camera.orthographic)
			Debug.LogError("Camera attached to game object (" + gameObject.name + ") should be orthographic. Scrolling will not work properly");

		m_isEnabled = false;
		m_velocity = Vector3.zero;
	}

	void Start()
	{
		if(autoEnable)
			Enable();
	}




	// Main coroutine loop.
	private IEnumerator c_Scroll()
	{
		while (true)
		{
			yield return StartCoroutine("c_DetectTouch");
			yield return StartCoroutine("c_CalculateVelocity");
			yield return StartCoroutine("c_ApplyVelocity");
		}
	}


	// Detects touch start.
	private IEnumerator c_DetectTouch()
	{
		while (true)
		{
			if(m_inputState == InputState.Down)
				break;

			yield return null;
		}
	}

	// While touching, detects velocity, moves camera with touch.move and limits camera movement.
	private IEnumerator c_CalculateVelocity()
	{
		Vector2 oldPos = m_cursorWorldPosition;
		Vector2 calculatedVelocity = Vector2.zero;
		float oldTime = Time.time;

		while (m_inputState == InputState.Down)
		{
			float now = Time.time;
			float elapsed = now - oldTime;
			Vector2 pos = m_cursorWorldPosition;
			Vector2 deltaPos = pos - oldPos;

			if (axis == Axis.XOnly)
				deltaPos.y = 0f;
			else if(axis == Axis.YOnly)
				deltaPos.x = 0f;

			Vector2 curVelocity = deltaPos / (1f+elapsed);
			calculatedVelocity = 0.8f*curVelocity + 0.2f*calculatedVelocity;

			transform.position -= new Vector3(deltaPos.x, deltaPos.y);

			oldTime = now;
			oldPos = m_cursorWorldPosition;

			yield return null;
		}

		m_velocity = calculatedVelocity;
	}

	// After touch ended, continues to move camera with momentum.
	private IEnumerator c_ApplyVelocity()
	{
		float oldTime = Time.time;

		while (m_inputState == InputState.Up)
		{
			float now = Time.time;
			float elapsed = now - oldTime;

			m_velocity = m_velocity * Mathf.Exp(-elapsed / 1f-drag);

			transform.position -= m_velocity;

			oldTime = now;


			yield return new WaitForFixedUpdate();
		}
	}




	// Enables scrolling
	public void Enable()
	{
		if (m_isEnabled)
			return;
		m_isEnabled = true;

		StartCoroutine("c_Scroll");
	}

	//Disables scrolling
	public void Disable()
	{
		if (! m_isEnabled)
			return;
		m_isEnabled = false;

		StopAllCoroutines();
	}

	//TODO limit bounds
	//TODO bounce on hitting bounds
	//TODO movement events
}
