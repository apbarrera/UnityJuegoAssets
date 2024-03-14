using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    static GameController current;
    
    [SerializeField] private GameObject fundidoNegro;
    [SerializeField] private Text contadorMonedas;
    [SerializeField] private GameObject camara;


    public static bool gameOn = false;
    private Image sprFundido;
    public static bool playerMuerto;
    private AudioSource musicaFondo;

    private int monedas;

    public static void SumaMonedas()
    {
        current.monedas++;
        if (current.monedas < 10) current.contadorMonedas.text = "0" + current.monedas;
        else current.contadorMonedas.text = current.monedas.ToString();
    }

    private void Awake()
    {
        current = this;
        fundidoNegro.SetActive(true);
    }

    private void Start()
    {
        sprFundido = fundidoNegro.GetComponent<Image>();
        musicaFondo = camara.GetComponent<AudioSource>();
        Invoke("QuitaFundido", 0.5f);
    }

    private void Update()
    {
        if (playerMuerto)
        {
            musicaFondo.Stop();
            StartCoroutine("PonerFC");
            playerMuerto = false;
        }
    }

    private void QuitaFundido()
    {
        StartCoroutine("QuitaFC");
    }

    IEnumerator QuitaFC()
    {
        for (float alpha = 1f; alpha >= 0; alpha -= Time.deltaTime * 2f)
        {
            sprFundido.color = new Color(sprFundido.color.r, sprFundido.color.g, sprFundido.color.b, alpha);
            yield return null;
        }
        gameOn = true;
        musicaFondo.Play();
    }

    IEnumerator PonerFC()
    {
        for (float alpha = 0f; alpha <= 1; alpha += Time.deltaTime * 2f)
        {
            sprFundido.color = new Color(sprFundido.color.r, sprFundido.color.g, sprFundido.color.b, alpha);
            yield return null;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
