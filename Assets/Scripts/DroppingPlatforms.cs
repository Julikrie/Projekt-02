using System.Collections;
using UnityEngine;
using Cinemachine;

public class DroppingPlatforms : MonoBehaviour
{
    public GameObject PlatformCrumble;
    public float ShakeForce = 1f;
    public float DropDelay;
    public float RespawnTime;
    public float RumbleSoundLength;
    public AudioClip RumbleSound;

    private Rigidbody2D _rb;
    private Vector3 _originalPosition;
    private AudioSource _audioSource;
    private CinemachineImpulseSource _impulseSource;

    public void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _originalPosition = transform.position;
        _audioSource = GetComponent<AudioSource>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        PlatformCrumble.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(PlayRumbleForSeconds());
            StartCoroutine(DroppingPlatform());
            StartCoroutine(RespawnPlatform());
            _impulseSource.GenerateImpulseWithForce(ShakeForce);
            PlatformCrumble.SetActive(true);
        }
        else
        {
            PlatformCrumble.SetActive(false);
        }
    }

    // Sets the body to Dynamic, plattform is falling after Drop Delay
    private IEnumerator DroppingPlatform()
    {
        yield return new WaitForSeconds(DropDelay);
        _rb.bodyType = RigidbodyType2D.Dynamic;
    }

    // Resets the platform after Respawn Time
    private IEnumerator RespawnPlatform()
    {
        yield return new WaitForSeconds(RespawnTime);
        transform.position = _originalPosition;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.velocity = Vector3.zero;
    }

    // Plays the rumble for Rumble Sound Length time
    IEnumerator PlayRumbleForSeconds()
    {
        _audioSource.clip = RumbleSound;
        _audioSource.volume = 1f;
        _audioSource.Play();

        yield return new WaitForSeconds(RumbleSoundLength);
        _audioSource.Stop();
    }
}
