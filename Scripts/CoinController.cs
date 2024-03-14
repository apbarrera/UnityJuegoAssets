using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    private ParticleSystem particulas;
    private SpriteRenderer spr;
    private bool activa = true;
    private AudioSource sonido;

    private void Awake()
    {
        particulas = GetComponent<ParticleSystem>();
        spr = GetComponent<SpriteRenderer>();
        sonido = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && activa)
        {
            GameController.SumaMonedas();
            spr.enabled = false;
            particulas.Play();
            activa = false;
            sonido.Play();
        }
    }
}
