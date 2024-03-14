using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivaMensaje : MonoBehaviour
{
    [SerializeField] private GameObject mensaje;
    private SpriteRenderer spr;

    private void Start()
    {
        spr = mensaje.GetComponent<SpriteRenderer>();
        Color c = spr.material.color;
        c.a = 0f;
        spr.material.color = c;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            StartCoroutine("FadeIn");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            StartCoroutine("FadeOut");
        }
    }

    IEnumerator FadeIn()
    {
        for (float f = 0.0f; f <= 1; f += 0.02f)
        {
            if (spr != null)
            {
                Color c = spr.material.color;
                c.a = f;
                spr.material.color = c;
                yield return (0.09f);
            }
        }
    }

    IEnumerator FadeOut()
    {
        for (float f = 1; f >= 0; f -= 0.02f)
        {
            if (spr != null)
            {
                Color c = spr.material.color;
                c.a = f;
                spr.material.color = c;
                yield return (0.09f);
            }
        }
    }
}
