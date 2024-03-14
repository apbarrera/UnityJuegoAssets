using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour
{
    [Header("VALORES CONFIGURABLES")]
    [SerializeField] private int vida = 3;
    [SerializeField] private float velocidad;
    [SerializeField] private float fuerzaSalto;
    [SerializeField] private bool saltoMejorado;
    [SerializeField] private float saltoLargo = 1.5f;
    [SerializeField] private float saltoCorto = 1f;
    [SerializeField] private Transform checkGround;
    [SerializeField] private float checkGroundRadio;
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private float addRayoDebajo;
    [SerializeField] private float addRayoDelante;
    [SerializeField] private float addRayoDetras;
    [SerializeField] private float anguloMax;
    [SerializeField] private float fuerzaToque;

    [Header("BARRA DE VIDA")]
    [SerializeField] private GameObject barraVida;
    [SerializeField] private Sprite vida3, vida2, vida1, vida0;

    [Header("VALORES INFORMATIVOS")]
    [SerializeField] private bool tocaSuelo = false;
    [SerializeField] private bool enPendiente;
    [SerializeField] private bool puedoAndar;
    [SerializeField] private float anguloPendiente;
    [SerializeField] private float h;

    [Header("EFECTOS DE SONIDO")]
    [SerializeField] private GameObject oSaltoPlayer;
    [SerializeField] private GameObject oMuertePlayer;

    private Rigidbody2D rPlayer;
    private Animator aPlayer;
    private CapsuleCollider2D ccPlayer;
    private SpriteRenderer sPlayer;
    private Vector2 ccSize;
    private Camera camara;

    //private bool miraDerecha = true;
    private float dirX = 1;
    private bool saltando = false;
    private bool puedoSaltar = false;
    private bool enPlataforma = false;
    private Vector2 nuevaVelocidad;

    private Vector2 anguloPer;

    private bool tocando = false;
    private Color colorOriginal;
    private bool muerto = false;
    private float posPlayer, altoCam, altoPlayer;

    private float gravedad;
    private bool enAnguloMax;
    private RaycastHit2D hitDelante, hitDetras;
    private Vector3 posRayoDelante, posRayoDetras;
    private float anguloDelante, anguloDetras;

    private AudioSource sSaltoPlayer;
    private AudioSource sMuertePlayer;

    private Vector3 posIni;

    // Start is called before the first frame update
    void Start()
    {
        rPlayer = GetComponent<Rigidbody2D>();
        aPlayer = GetComponent<Animator>();
        ccPlayer = GetComponent<CapsuleCollider2D>();
        sPlayer = GetComponent<SpriteRenderer>();
        ccSize = ccPlayer.size;
        colorOriginal = sPlayer.color;
        camara = Camera.main;
        altoCam = camara.orthographicSize * 2;
        altoPlayer = GetComponent<Renderer>().bounds.size.y;
        gravedad = rPlayer.gravityScale;
        sSaltoPlayer = oSaltoPlayer.GetComponent<AudioSource>();
        sMuertePlayer = oMuertePlayer.GetComponent<AudioSource>();
        posIni = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        aPlayer.SetBool("GameOn", GameController.gameOn);
        if (GameController.gameOn)
        {
            recibePulsaciones();
            variablesAnimador();
        }
        if (muerto)
        {
            posPlayer = camara.transform.InverseTransformDirection(transform.position - camara.transform.position).y;
            if (posPlayer < ((altoCam/2) * -1) -(altoPlayer / 2)) {
                Invoke("llameRecarga", 1);
                muerto = false;
            }
        }
    }

    private void llameRecarga()
    {
        GameController.playerMuerto = true;
    }

    void FixedUpdate()
    {
        if (GameController.gameOn)
        {
            checkTocaSuelo();
            if (!enPlataforma) CheckPendiente();
            if (!tocando) movimientoPlayer();
        }
    }


    private void CheckPendiente()
    {
        enPendiente = false;
        rPlayer.gravityScale = gravedad;
        enAnguloMax = false;
        RaycastHit2D hitDebajo = Physics2D.Raycast(transform.position, Vector2.down, (ccSize.y / 2) + addRayoDebajo, capaSuelo);
        if (hitDebajo)
        {
            anguloPendiente = Vector2.Angle(hitDebajo.normal, Vector2.up);
            if (Mathf.Abs(anguloPendiente) <= anguloMax)
            {
                anguloPer = Vector2.Perpendicular(hitDebajo.normal).normalized;
                if ((anguloPer.y < 0 && dirX == -1) || (anguloPer.y > 0 && dirX == 1)) anguloPendiente *= -1;
                if (anguloPendiente != 0 && Mathf.Abs(anguloPendiente) <= anguloMax && tocaSuelo)
                {
                    enPendiente = true;
                    rPlayer.gravityScale = 0;
                }
                Debug.DrawRay(transform.position, Vector2.down * ((ccSize.y / 2) + addRayoDebajo), Color.red);
                Debug.DrawRay(hitDebajo.point, anguloPer, Color.blue);
                Debug.DrawRay(hitDebajo.point, hitDebajo.normal, Color.green);
            }
            else enAnguloMax = true;
        }

        posRayoDelante = new Vector3(transform.position.x + ((ccSize.x / 2) * dirX), transform.position.y);
        posRayoDetras = new Vector3(transform.position.x - ((ccSize.x / 2) * dirX), transform.position.y);
        hitDelante = Physics2D.Raycast(posRayoDelante, Vector2.down, addRayoDelante, capaSuelo);
        hitDetras = Physics2D.Raycast(posRayoDetras, Vector2.down, addRayoDetras, capaSuelo);
        if (hitDelante && hitDetras)
        {
            Debug.DrawRay(posRayoDelante, Vector2.down * addRayoDelante, Color.yellow);
            Debug.DrawRay(posRayoDetras, Vector2.down * addRayoDetras, Color.black);
            anguloDelante = Vector2.Angle(hitDelante.normal, Vector2.up);
            anguloDetras = Vector2.Angle(hitDetras.normal, Vector2.up);
        }
    }

    private void movimientoPlayer()
    {
        if (enPendiente && !saltando) nuevaVelocidad.Set(velocidad * anguloPer.x * -h, velocidad * anguloPer.y * -h);
        else nuevaVelocidad.Set(velocidad * h, rPlayer.velocity.y);
        rPlayer.velocity = nuevaVelocidad;

        if ((anguloDelante != anguloDetras) && (hitDelante.point.y > hitDetras.point.y) && anguloDetras != 0 && h != 0 && !saltando)
        {
            nuevaVelocidad = new Vector2(rPlayer.velocity.x, 0.0f);
            rPlayer.velocity = nuevaVelocidad;
        }
        if (enAnguloMax) rPlayer.velocity = new Vector2(0.0f, -velocidad);
    }

    private void recibePulsaciones()
    {
        if (Input.GetKey(KeyCode.R)) GameController.playerMuerto = true; //VOLVER A COLOCAR AL JUGADOR EN LA POSICION INICIAL
        h = Input.GetAxisRaw("Horizontal");
        if ((h > 0 && dirX == -1) || (h < 0 && dirX == 1)) giraPlayer();
        if (Input.GetButtonDown("Jump") && puedoSaltar && tocaSuelo && !enAnguloMax) Salto();
        if (saltoMejorado) SaltoMejorado();
    }

    private void Salto()
    {
        saltando = true;
        puedoSaltar = false;
        rPlayer.velocity = new Vector2(rPlayer.velocity.x, 0f);
        rPlayer.AddForce(new Vector2(0, fuerzaSalto), ForceMode2D.Impulse);
        sSaltoPlayer.Play();
    }

    private void SaltoMejorado()
    {
        if (rPlayer.velocity.y < 0)
        {
            rPlayer.velocity += Vector2.up * Physics2D.gravity.y * saltoLargo * Time.deltaTime;

        }
        else if (rPlayer.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rPlayer.velocity += Vector2.up * Physics2D.gravity.y * saltoCorto * Time.deltaTime;
        }
    }

    private void checkTocaSuelo()
    {
        tocaSuelo = Physics2D.OverlapCircle(checkGround.position, checkGroundRadio, capaSuelo);
        if (rPlayer.velocity.y <= 0f)
        {
            saltando = false;
            if (tocando && tocaSuelo)
            {
                rPlayer.velocity = Vector2.zero;
                tocando = false;
                sPlayer.color = colorOriginal;
            }
        }
        if (tocaSuelo && !saltando)
        {
            puedoSaltar = true;

        }
    }

    //DETECCION DE PLATAFORMAS
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "PlataformaMovil" && !muerto)
        {
            rPlayer.velocity = Vector3.zero;
            transform.parent = collision.transform;
            enPlataforma = true;
        }
        if (collision.gameObject.tag == "EnemigoHerida" && !muerto)
        {
            tocado(collision.transform.position.x);
        }
        if (collision.gameObject.tag == "EspaldaEnemigo" && !tocando && !muerto)
        {
            rPlayer.velocity = Vector2.zero;
            rPlayer.AddForce(new Vector2(0.0f, 10f), ForceMode2D.Impulse);
            collision.gameObject.SendMessage("Muere");
        }

    }

    private void tocado(float posX)
    {
        if (!tocando)
        {
            if (vida > 1)
            {
                Color nuevoColor = new Color(255f / 255f, 100f / 255f, 100f / 255f);
                sPlayer.color = nuevoColor;
                tocando = true;
                float lado = Mathf.Sign(posX - transform.position.x);
                rPlayer.velocity = Vector2.zero;
                rPlayer.AddForce(new Vector2(fuerzaToque * -lado, fuerzaToque), ForceMode2D.Impulse);
                vida--;
                BarraVida(vida);
            }
            else
            {
                muertePlayer();
            }
        }
    }


    private void BarraVida(int salud)
    {
        if (salud == 2) barraVida.GetComponent<Image>().sprite = vida2;
        if (salud == 1) barraVida.GetComponent<Image>().sprite = vida1;
    }


    private void muertePlayer()
    {
        sMuertePlayer.Play();
        barraVida.GetComponent<Image>().sprite = vida0;
        aPlayer.Play("Muerto");
        GameController.gameOn = false;
        rPlayer.velocity = Vector2.zero;
        rPlayer.AddForce(new Vector2(0.0f, fuerzaSalto), ForceMode2D.Impulse);
        ccPlayer.enabled = false;
        muerto = true;

    }

    //FIN DE DETECCION DE PLATAFORMAS MOVILES
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Pinchos" && !muerto)
        {
            muertePlayer();
        }
        if (collision.gameObject.tag == "CaídaAlVacío")
        {
            Invoke("llameRecarga", 1);
            muertePlayer();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "PlataformaMovil" && !muerto)
        {
            transform.parent = null;
            enPlataforma = false;
        }
    }

    private void variablesAnimador()
    {
        aPlayer.SetFloat("VelocidadX", Mathf.Abs(rPlayer.velocity.x));
        aPlayer.SetFloat("VelocidadY", rPlayer.velocity.y);
        aPlayer.SetBool("Saltando", saltando);
        aPlayer.SetBool("TocaSuelo", tocaSuelo);
    }

    void giraPlayer()
    {
        dirX *= -1;
        Vector3 escalaGiro = transform.localScale;
        escalaGiro.x = escalaGiro.x * -1;
        transform.localScale = escalaGiro;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(checkGround.position, checkGroundRadio);
    }

}