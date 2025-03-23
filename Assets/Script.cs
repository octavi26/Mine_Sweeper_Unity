using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script : MonoBehaviour
{
    const int MAX = 1000;

    public int lines = 0, colums = 0;
    public float squareLenght = 1;
    public float marginWidth = 0.05f;
    public float bombChance = 20; 
    public GameObject square;
    public GameObject mainSquare;
    private GameObject[,] Square = new GameObject[MAX,MAX];
    private bool[,] Clicked = new bool[MAX, MAX];
    private bool[,] Flag = new bool[MAX, MAX];
    private bool[,] Bomb = new bool[MAX, MAX];
    private int[,] A = new int[MAX, MAX];

    struct pos{
        public int x, y;
    };

    private int[] dx = {0, 1, 0, -1, -1, -1, 1, 1};
    private int[] dy = {1, 0, -1, 0, 1, -1, 1, -1};
    
    void Start()
    {
        square.transform.localScale = new Vector3(1, 1, 1) * squareLenght - new Vector3(1, 1, 1) * marginWidth;
        mainSquare.GetComponent<SpriteRenderer>().size = new Vector2(colums, lines) * squareLenght + new Vector2(.1f, .1f);

        int i, j;
        for( i=0; i<lines; i++ )
            for( j=0; j<colums; j++ ){
                Vector2 position = new Vector2(0f, 0f);
                position.y = (float)lines / 2 - i - 0.5f;
                position.x = (float)colums / 2 - j - 0.5f;
                position *= squareLenght;
                Square[i, j] = Instantiate(square, position, Quaternion.identity, gameObject.transform);
            }

        GenerateMatrix();
    }

    // Update is called once per frame
    void Update()
    {
        if( Input.GetMouseButtonDown(0) ) Click(0);
        if( Input.GetMouseButtonDown(1) ) Click(1);
        if( Input.GetKeyDown(KeyCode.R) ) ResetMatrix();
        ColorMatrix();
    }

    void ColorMatrix(){
        for( int i=0; i<lines; i++ )
            for( int j=0; j<colums; j++ )
                if( Clicked[i, j] == true ) Square[i,j].GetComponent<SpriteRenderer>().color = new Color(0.4150943f, 0.4150943f, 0.4150943f);
                else Square[i,j].GetComponent<SpriteRenderer>().color = new Color(0.1882353f, 0.1882353f, 0.1882353f);
                
        for( int i=0; i<lines; i++ )
            for( int j=0; j<colums; j++ )
                if( Clicked[i, j] ){
                    if( Bomb[i, j] ) Square[i, j].transform.GetChild(0).gameObject.SetActive(true);
                    if( Flag[i, j] ) Square[i, j].transform.GetChild(1).gameObject.SetActive(false);
                    if( A[i, j] >= 1 && !Bomb[i, j] ) Square[i, j].transform.GetChild(1 + A[i, j]).gameObject.SetActive(true);
                }
                else{
                    foreach(Transform child in Square[i, j].transform)
                        child.gameObject.SetActive(false);
                    if( Flag[i, j] ) Square[i, j].transform.GetChild(1).gameObject.SetActive(true);
                }
    }

    void ResetMatrix(){
        for( int i=0; i<lines; i++ )
            for( int j=0; j<colums; j++ )
                foreach(Transform child in Square[i, j].transform)
                    child.gameObject.SetActive(false);

        for( int i=0; i<lines; i++ )
            for( int j=0; j<colums; j++ ){
                Clicked[i, j] = false;
                Flag[i, j] = false;
                Bomb[i, j] = false;
                A[i, j] = 0;
            }
        GenerateMatrix();
    }

    void Fill( int i, int j ){
        for( int k=0; k<4; k++ ){
            int x = i + dx[k];
            int y = j + dy[k];
            if( InMatrix(x, y) && !Bomb[x, y] && !Clicked[x, y] ){
                Clicked[x, y] = true;
                if( A[x, y] == 0 ) Fill(x, y);
            }
        }
    }

    void Click(int click){
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 boundingBox = (mainSquare.GetComponent<SpriteRenderer>().size  - new Vector2(.1f, .1f)) / 2f;
        if( mousePos.y < -boundingBox.y || mousePos.y > boundingBox.y || mousePos.x < -boundingBox.x || mousePos.x > boundingBox.x ) return;

        int i = lines - (int)((mousePos.y + boundingBox.y) / squareLenght) - 1;
        int j = colums - (int)((mousePos.x + boundingBox.x) / squareLenght) - 1;


        if( click == 1 ) Flag[i, j] = !Flag[i, j];
        if( click == 0 ) Clicked[i, j] = true;
        if( Clicked[i, j] == true && !Bomb[i, j] && A[i, j] == 0 ) Fill(i, j);
        //Debug.Log(i + " " + j + ": " + NearbyBombs(i, j));
    }

    bool InMatrix( int x, int y ){
        return !(x < 0 || x >= lines || y < 0 || y >= colums);
    }

    int NearbyBombs( int i, int j ){
        int counter = 0;
        for( int k=0; k<8; k++ ){
            int x = i + dx[k];
            int y = j + dy[k];
            //if(InMatrix(x, y) )Debug.Log("IN NearbyBombs: " + x + " " + y + " " + Bomb[x, y]);
            if( InMatrix(x, y) && Bomb[x, y] ) counter++;
        }
        return counter;
    }

    void GenerateMatrix(){
        for( int i=0; i<lines; i++ )
            for( int j=0; j<colums; j++ ){
                float rand = Random.Range(0f, 100f);
                float thisBombChance = bombChance;
                if( NearbyBombs(i, j) > 0 ) thisBombChance *= 1.25f;
                if( rand <= thisBombChance ) Bomb[i, j] = true;
            }
        
        for( int i=0; i<lines; i++ )
            for( int j=0; j<colums; j++ )
                A[i, j] = NearbyBombs(i, j);
    }
}
