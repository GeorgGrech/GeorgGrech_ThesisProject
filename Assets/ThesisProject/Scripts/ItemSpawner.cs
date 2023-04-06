//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ItemSpawner : MonoBehaviour
{
    public List<GameObject> ResourceObjects; //List of all resource objects in scene, to be used when training agent

    //[SerializeField] public bool agentTrainingLevel = false; //if level is used for training

    //[SerializeField] private GameObject levelTerrainObject; //Level Terrain GameObject
    [SerializeField] private Terrain levelTerrain;
    private float terrainSizeX;
    private float terrainSizeZ;

    [SerializeField] private GameObject playerBase; //Player base, where resources are dropped off by player
    [SerializeField] private GameObject enemyBase; //Enemy base, where resources are dropped off by enemy agent

    //[SerializeField] private GameObject playerPrefab; //Player prefab
    [SerializeField] private GameObject enemyPrefab; //Enemy agent

    //[SerializeField] private GameObject resource1; //First resource - eg. Tree
    //[SerializeField] private GameObject resource2; //First resource - eg. Iron Mine
    [SerializeField] private GameObject[] resources; //List of resources
    private GameObject ResourceParent; //World object to contain all resource gameobjects for tidiness

    [SerializeField] private int resourceSpacing; //Spacing between resources
    [SerializeField] private float resourceSpawnShift; //Amount of random shift from original resource spawn
    [SerializeField] private float baseDistance; //Minimum empty distance around bases

    private GameObject ObstacleParent; //World object to contain all resource gameobjects for tidiness
    [SerializeField] private bool spawnObstacles; //Obstacles that spawn in world
    [SerializeField] private GameObject[] obstacles; //Obstacles that spawn in world
    [SerializeField] private int obstacleSpacing; //Spacing between obstacles
    [SerializeField] private float obstacleSpawnShift; //Amount of random shift from original obstacle spawn
    [SerializeField] private float obstacleMinResize; //Min amount possible of x/z scaling
    [SerializeField] private float obstacleMaxResize; //Max amount possible of x/z scaling
    [SerializeField] private float obstacleExtraBaseSpacing; //Increase of baseDistance especially for obstacles

    [SerializeField] private float baseCenterOffset; //Max random offset from center of map for base spawn
    [SerializeField] private float playerSpawnDistance; //Spawn distance for both player and enemy for their respective bases

    private Vector3 playerBaseLocation; //Location in world of Player Base;
    private Vector3 enemyBaseLocation; //Location in world of Enemy Base;

    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.itemSpawner = this;

        //gameManager.agentTrainingLevel = agentTrainingLevel;

        terrainSizeX = levelTerrain.terrainData.size.x;
        terrainSizeZ = levelTerrain.terrainData.size.z;

        if (gameManager.levelType == GameManager.LevelType.PlayerLevel) //Don't attempt to relocate player and spawn player base if level is just for agent training/evaluation
        {
            SpawnPlayerBase(); //Spawn Player Base
            RelocatePlayer(); //Move player to 
        }

        SpawnEnemyBase(); //Spawn Enemy Base taking note of Player Base location
        SpawnResources(); //Spawn Resources
        SpawnObstacles();
        SpawnEnemy(); //Spawn Enemy near Enemy Base

        //StartCoroutine(gameManager.Timer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnPlayerBase()
    {
        GameObject instantiatedBase = Instantiate(playerBase, GenerateRandomBaseSpawn(), Quaternion.identity);
        playerBaseLocation = instantiatedBase.transform.position;
    }

    private void SpawnEnemyBase()
    {
        Vector3 spawnLocation;
        do
        {
            spawnLocation = GenerateRandomBaseSpawn();
        } while (Vector3.Distance(spawnLocation,playerBaseLocation)<baseDistance); //Check that spawn location is not too close to enemy base

        GameObject instantiatedBase = Instantiate(enemyBase, spawnLocation, Quaternion.identity);
        enemyBaseLocation = instantiatedBase.transform.position;
    }

    private void SpawnResources()
    {

        ResourceParent = new GameObject
        {
            name = "Resources"
        };

        for (int z = 0; z < terrainSizeZ; z += resourceSpacing)
        {
            for (int x = 0; x < terrainSizeX; x += resourceSpacing)
            {
                
                //if (treeInstanceList.Count < maxTrees)
                //{
                    //Vector3 spawnPos = new Vector3(x, terrainData.size.y, z);

                    //float currentHeight = Terrain.activeTerrain.SampleHeight(spawnPos) / terrainData.size.y; //give us a height value between 0 & 1

                    //if (currentHeight >= treeData[treeIndex].minHeight && currentHeight <= treeData[treeIndex].maxHeight)
                    //{
                float randomX = (x + Random.Range(-resourceSpawnShift, resourceSpawnShift)) / terrainSizeX; 
                float randomZ = (z + Random.Range(-resourceSpawnShift, resourceSpawnShift)) / terrainSizeZ; //Shift position slightly to be more natural

                Vector3 resourcePosition = new Vector3(randomX * terrainSizeX,
                                                    /*currentHeight * terrainData.size.y,*/ 0,
                                                    randomZ * terrainSizeX) + this.transform.position;

                if((Vector3.Distance(resourcePosition,playerBaseLocation)>baseDistance)&& //Distant from Player Base
                    (Vector3.Distance(resourcePosition, enemyBaseLocation) > baseDistance)) //Distant from Enemy Base
                {
                    GameObject resource;

                    int randomNum = Random.Range(0, 20); //Random number to decide what resource
                    if (randomNum == 0)
                    {
                        resource = Instantiate(resources[4], resourcePosition, Quaternion.identity,ResourceParent.transform); //Gold - Rare (1 in 20)
                    }
                    else if(randomNum < 5)
                    {
                        resource = Instantiate(resources[3], resourcePosition, Quaternion.identity, ResourceParent.transform); //Iron - Uncommon (4 in 20)
                    }
                    else
                    {
                        int treetype = Random.Range(0, 3); //Spawn random one of 3 tree models
                        resource = Instantiate(resources[treetype], resourcePosition, Quaternion.identity, ResourceParent.transform); // Wood - Common (15 in 20)
                    } 

                    Quaternion spawnRotation = Quaternion.Euler(0, Random.Range(0, 360), 0); //Random y rotation

                    Transform mesh = resource.transform.Find("ResourceMesh"); //Only rotate mesh to not interfere with UI
                    mesh.localRotation = spawnRotation;
                }

                /*
                        RaycastHit raycastHit;

                        int layerMask = 1 << terrainLayerIndex;

                        if (Physics.Raycast(treePosition, -Vector3.up, out raycastHit, 100, layerMask) ||
                            Physics.Raycast(treePosition, Vector3.up, out raycastHit, 100, layerMask))
                        {
                            float treeDistance = (raycastHit.point.y - this.transform.position.y) / terrainData.size.y;

                            TreeInstance treeInstance = new TreeInstance();

                            treeInstance.position = new Vector3(randomX, treeDistance, randomZ);
                            treeInstance.rotation = UnityEngine.Random.Range(0, 360);
                            treeInstance.prototypeIndex = treeIndex;
                            treeInstance.color = Color.white;
                            treeInstance.lightmapColor = Color.white;
                            treeInstance.heightScale = 0.95f;
                            treeInstance.widthScale = 0.95f;

                            treeInstanceList.Add(treeInstance);
                        }
                    }*/
                //}
            }
        }
    }

    void RelocatePlayer() //Find Player in world and move him near player base
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = new Vector3((playerBaseLocation.x - playerSpawnDistance), playerBaseLocation.y, playerBaseLocation.z);
    }

    void SpawnEnemy() //Spawn Enemy agent
    {
        GameObject enemy = Instantiate(enemyPrefab, new Vector3((enemyBaseLocation.x - playerSpawnDistance), enemyBaseLocation.y, enemyBaseLocation.z),Quaternion.identity);
        gameManager.enemyAgent = enemy.GetComponent<EnemyAgent>();
    }

    public void ResetLevel(GameObject enemy, bool newEnemy) //Used by Enemy Agent for resetting level for new episode
    {
        //itemSpawner = GameObject.Find("ItemSpawner").GetComponent<ItemSpawner>();

        Destroy(ResourceParent);
        Destroy(ObstacleParent);
        /*
        foreach(GameObject resourceObject in ResourceObjects)
        {
            Destroy(resourceObject);
        }*/
        ClearNullValues();
 
        SpawnResources(); //Spawn Resources
        SpawnObstacles();

        if (!newEnemy)
            RelocateEnemy(enemy);
        else
        {
            Destroy(enemy);
            SpawnEnemy();
        }


        gameManager.ResetScoreText();
        StartCoroutine(gameManager.Timer());
    }

    public void RelocateEnemy(GameObject enemy) //Relocate enemy
    {
        Debug.Log("Relocating enemy");
        enemy.transform.position = new Vector3((enemyBaseLocation.x - playerSpawnDistance), enemyBaseLocation.y, enemyBaseLocation.z);
    }

    private Vector3 GenerateRandomBaseSpawn()
    {
        Vector3 spawnLocation = new Vector3(Random.Range((terrainSizeX/2)-baseCenterOffset,(terrainSizeX/2)+baseCenterOffset),
            0, //Note: Spawn Location Y may be changed in case of shaped terrain, in which case obtain Y using Terrain.sampleHeight after first generation
            Random.Range((terrainSizeZ/2) - baseCenterOffset, (terrainSizeZ/2) + baseCenterOffset));

        return spawnLocation;
    }

    /*
    private Vector3 GenerateRandomSpawn()
    {
        Vector3 spawnLocation = new Vector3(Random.Range(0, terrainSizeX),
            0, //Note: Spawn Location Y may be changed in case of shaped terrain, in which case obtain Y using Terrain.sampleHeight after first generation
            Random.Range(0, terrainSizeZ));

        return spawnLocation;
    }
    */


    public void ClearNullValues()
    {
        ResourceObjects.RemoveAll(s => s == null);
    }


    private void SpawnObstacles()
    {
        if (spawnObstacles)
        {
            ObstacleParent = new GameObject
            {
                name = "Obstacles"
            };

            for (int z = 0; z < terrainSizeZ; z += obstacleSpacing)
            {
                for (int x = 0; x < terrainSizeX; x += obstacleSpacing)
                {

                    float randomX = (x + Random.Range(-obstacleSpawnShift, obstacleSpawnShift)) / terrainSizeX;
                    float randomZ = (z + Random.Range(-obstacleSpawnShift, obstacleSpawnShift)) / terrainSizeZ; //Shift position slightly to be more natural

                    Vector3 obstaclePosition = new Vector3(randomX * terrainSizeX,
                                                        /*currentHeight * terrainData.size.y,*/ 0,
                                                        randomZ * terrainSizeX) + this.transform.position;

                    if ((Vector3.Distance(obstaclePosition, playerBaseLocation) > baseDistance+obstacleExtraBaseSpacing) && //Distant from Player Base
                        (Vector3.Distance(obstaclePosition, enemyBaseLocation) > baseDistance+ obstacleExtraBaseSpacing)) //Distant from Enemy Base
                    {
                        Quaternion spawnRotation = Quaternion.Euler(0, Random.Range(0, 360), 0); //Random y rotation

                        GameObject ObstacleInstance = Instantiate(obstacles[Random.Range(0,obstacles.Length)], obstaclePosition, spawnRotation, ObstacleParent.transform); //Spawn obstacle from list

                        ObstacleInstance.transform.localScale = new Vector3(Random.Range(obstacleMinResize, obstacleMaxResize), 1, Random.Range(obstacleMinResize, obstacleMaxResize)); //Resize object x and z at random
                    }
                }
            }
        }
    }
}
