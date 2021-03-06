using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }



    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];

        //TODO: Inicializar matriz a 0's
        for (int i = 0; i < Constants.NumTiles; i++)
        {

            for (int j = 0; j < Constants.NumTiles; j++)
            {
                matriu[i, j] = 0;
            }

        }

        //TODO: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)
        for (int i = 0; i < Constants.NumTiles; i++)
        {
            //arriba
            if (i > 7) { matriu[i, i - 8] = 1; }
            // abajo

            if (i < 56) { matriu[i, i + 8] = 1; }

            //izquierda
            if (i % 8 != 0) { matriu[i, i - 1] = 1; }
            //derecha
            if (((i + 1) % 8) != 0) { matriu[i, i + 1] = 1; }
        }

        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes
        for (int i = 0; i < Constants.NumTiles; i++)
        {

            for (int j = 0; j < Constants.NumTiles; j++)
            {
                if (matriu[i, j] == 1)
                {
                    tiles[i].adjacency.Add(j);
                    /* Tiles es un array que contiene todas las casillas del tablero
                   Adjency  lista de indices de las casillas adyacentes
                   agregamos las adyacentes */
                }

            }



        }

    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }
    Dictionary<Tile, List<int>> adyacentesRobberConDistancia = new Dictionary<Tile, List<int>>();

    public void RobberTurn()
    {
       clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        //Movimiento Aleatorio del Ladron
        int numCasillasAdyacentes = tiles[robber.GetComponent<RobberMove>().currentTile].adjacency.Count - 1;
        Tile casillaRandomAdyacenteLadron = tiles[tiles[robber.GetComponent<RobberMove>().currentTile].adjacency[Random.Range(0, numCasillasAdyacentes)]];
        
        robber.GetComponent<RobberMove>().MoveToTile(casillaRandomAdyacenteLadron);
        robber.GetComponent<RobberMove>().currentTile = casillaRandomAdyacenteLadron.numTile;
    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
        int indexcurrentTile;

        if (cop == true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;
        tiles[indexcurrentTile].visited = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        //TODO:Implementar BFS. Los nodos seleccionables los ponemos como selectable=true

        // indices ocupados por los otros cops
        List<int> indicesCops = new List<int>();
        foreach (GameObject copTmp in cops)
        {
            indicesCops.Add(copTmp.GetComponent<CopMove>().currentTile);

            //Guardamos en un GameObject todos los cops y con GetComponent y currentFile,
            //podemos saber en que casilla estan en todo momentos los cops, almcenadolo en indiciesCops
        }


        foreach (int indice in tiles[indexcurrentTile].adjacency)
        {

            tiles[indice].parent = tiles[indexcurrentTile];
            nodes.Enqueue(tiles[indice]);//añadir a la cola de nodos


        }

        while (nodes.Count > 0)
        {
            Tile tmp = nodes.Dequeue();
            if (!tmp.visited)
            {
                // evitamos que no recorra la casilla del policia
                if (indicesCops.Contains(tmp.numTile))
                {
                    tmp.visited = true;
                    tmp.distance = tmp.parent.distance + 1;
                }
                else
                {
                    tmp.visited = true;
                    tmp.distance = tmp.parent.distance + 1;

                    foreach (int indice in tmp.adjacency)
                    {

                        // no puede ir a donde estan ocupadas por ningun cop, esto le incluye a ella misma
                        if (!tiles[indice].visited)
                        {
                            tiles[indice].parent = tmp;
                            nodes.Enqueue(tiles[indice]);
                        }

                    }
                }
            }
        }

        foreach (Tile t in tiles)
        {

            if (cop && adyacentesRobberConDistancia.Count > 0 && adyacentesRobberConDistancia.ContainsKey(t)) //calculamos la distancia de entre
                                                                                                              //las casillas y el policia

            {
                adyacentesRobberConDistancia[t].Add(t.distance);

            }
            //buscamos casillas que tengan una distancia 
            // menor a dos casillas.
            // no puede ir a donde estan ocupadas por ningun cop, esto le incluye a ella misma
            if (t.distance <= 2 && !indicesCops.Contains(t.numTile))
            {
                t.selectable = true;                                                                    //las convertimos a selectable
            }
        }

    }
    
   
    

    

   

       
}
