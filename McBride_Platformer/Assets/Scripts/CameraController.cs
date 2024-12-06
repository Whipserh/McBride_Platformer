using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController: MonoBehaviour
{
	public Transform target;
	public Tilemap tilemap;
	public float arrivalTime;
	public Camera followCamera;
	  
	private Vector3 currentVelocity;
	  
	private Vector2 viewportHalfSize;
	private float leftBoundary, rightBoundary, bottomBoundary;


	public Vector2 shakeOffset;

	public void Start()
	{
	tilemap.CompressBounds();
	CalculateBounds();
	}

	public void LateUpdate()
	{
		Vector3 desiredPosition = new Vector3(target.position.x,target.position.y,transform.position.z) + (Vector3)shakeOffset;
        /**
		Vector3 smoothPositon = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, arrivalTime);

		smoothPositon.x = Mathf.Clamp(smoothPositon.x, leftBoundary, rightBoundary);
        smoothPositon.y = Mathf.Clamp(smoothPositon.y, bottomBoundary, smoothPositon.y);
		transform.position = smoothPositon;

		comment out the code below
         **/


        desiredPosition.x = Mathf.Clamp(desiredPosition.x, leftBoundary, rightBoundary);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, bottomBoundary, desiredPosition.y);

        transform.position = desiredPosition;
    }	

	public void Shake(float intensity, float duration)
	{
		StartCoroutine(ShakeCorotine(intensity, duration));
	}

	private IEnumerator ShakeCorotine(float intensity, float duration)
	{
		float elapsed = 0f;

		while(elapsed < duration)
		{
			shakeOffset = Random.insideUnitCircle * intensity;
			elapsed += Time.deltaTime;
			yield return null;
		}
		//turn off shake
		shakeOffset = Vector2.zero;
	}


    private void CalculateBounds()
	{
	viewportHalfSize = new Vector2(followCamera.aspect * followCamera.orthographicSize, followCamera.orthographicSize);
	leftBoundary = tilemap.transform.position.x + tilemap.cellBounds.min.x + viewportHalfSize.x;
	rightBoundary = tilemap.transform.position.x + tilemap.cellBounds.max.x - viewportHalfSize.x;
	bottomBoundary = tilemap.transform.position.y + tilemap.cellBounds.min.y + viewportHalfSize.y;
	}
}