﻿using System.Collections;
using UnityEngine;

/* Models player object behavior, such as what to do when
 * a player collides and managing live count. */
public class Player : MonoBehaviour
{
    [SerializeField]
    private float respawnDelay = 5f;
    [SerializeField]
    private float respawnInvincibilityDuration = 2f;
    [SerializeField]
    private GameObject playerDeathParticleSystem;
    [SerializeField]
    private GameObject playerCollisionParticleSystem;
    [SerializeField]
    private int lives = 5;

    private bool hidden = false;
    private float liveTime = 0; // How long has the player been alive?
    private static int playerCount = 0; // Counts how many instances are alive (mod 12).
    private int playerIndex = 0;
    private Color playerColor;


    /* Choose random iris color at start. */
    public void Start()
    {
        playerIndex = playerCount;
        playerCount = (playerCount + 1) % 12;
        float secondRoundOffset = playerIndex > 8 ? 0.0f : 0.05f; // ninth player hue in-between playerIndex 0's hue and playerIndex 1's hue.

        playerColor = Color.HSVToRGB(playerIndex / 8.0f + secondRoundOffset, 1.0f, 0.7f);
        transform.Find("Iris").GetComponent<Renderer>().material.SetColor("_Color", playerColor);
        transform.Find("Iris").GetComponent<TrailRenderer>().startColor = playerColor;//.material.SetColor("_Color", irisColor);
    }

    /* Every frame, if the player is alive, increase the livetime. */
    public void Update()
    {
        if (lives > 0)
            this.liveTime += Time.deltaTime;
    }

    /* Do not allow the player to live outside of the screen. */
    public void OnBecameInvisible()
    {
        if (!hidden)
            kill();
    }

    /* When the player collides with another physics object, either
     * die or add some nice particles. */
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (!hidden && coll.gameObject.tag == "Enemy")
        {
            this.hidden = true;
            GameObject.Destroy(coll.gameObject);
            kill();
        }
        else if (!hidden && coll.gameObject.tag == "Player")
        {
            GameObject particles = Instantiate(playerCollisionParticleSystem, Camera.main.transform);
            particles.transform.position = coll.transform.position;
        }
    }

    /* The player loses a live. */
    public void kill()
    {
        lives = Mathf.Max(0, lives - 1);
        GameObject particles = Instantiate(playerDeathParticleSystem, Camera.main.transform);
        particles.GetComponent<Renderer>().material.SetColor("_EmissionColor", playerColor);
        particles.GetComponent<Renderer>().material.SetColor("_Color", playerColor);

        particles.transform.position = gameObject.transform.position;
        Vector2 LookAtPoint = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y);
        particles.transform.LookAt(LookAtPoint);
        string particle = lives.ToString();

        if (lives < 1)
            particle = "skull";
        else
            StartCoroutine("Respawn", respawnDelay);

        particles.GetComponent<ParticleSystemRenderer>().material.mainTexture = Resources.Load("Textures/Lives/" + particle) as Texture;
        ShowPlayer(false);
        EnableCollider(false);

        if (lives < 0)
            FindObjectOfType<AudioReverbFilter>().reverbLevel = -10000;
        else if (lives >=1 && lives <= 5)
            FindObjectOfType<AudioReverbFilter>().reverbLevel = 1500;
        
        
    }

    /* Hide the player (visually) from the screen. */
    public void ShowPlayer(bool show)
    {
        this.GetComponent<MeshRenderer>().enabled = show;
        foreach (MeshRenderer renderer in this.GetComponentsInChildren<MeshRenderer>())
            renderer.enabled = show;
        foreach (TrailRenderer renderer in this.GetComponentsInChildren<TrailRenderer>())
            renderer.enabled = show;
    }

    /* Place the player, after a spawnDelay, back in the game, visible and all.
     * The collider will be enabled a bit later, to allow for invincability. */
    IEnumerator Respawn(float spawnDelay)
    {
        yield return new WaitForSeconds(spawnDelay);
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
        Vector3 newPos = new Vector3(camera.transform.position.x, 
                                     camera.transform.position.y, transform.position.z);
        gameObject.transform.position = newPos;
        ShowPlayer(true);
        this.hidden = false;
        FindObjectOfType<AudioReverbFilter>().reverbLevel = 0;
        GetComponent<Renderer>().material.color = Color.red;
        PulseAnimation anim = gameObject.AddComponent<PulseAnimation>();
        anim.SetFrequency(10);
        anim.SetMinScale(0.9f);
        anim.SetAmplitude(0.2f);
        yield return new WaitForSeconds(respawnInvincibilityDuration);
        GetComponent<Renderer>().material.color = Color.white;
        Destroy(anim);
        EnableCollider(true);
    }

    public void EnableCollider(bool toggle)
    {
        this.GetComponent<CircleCollider2D>().enabled = toggle;
    }

    public int GetLives()
    {
        return lives;
    }

    public float GetLiveTime()
    {
        return liveTime;
    }
}
