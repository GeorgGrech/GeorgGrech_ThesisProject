//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ItemSpawner : MonoBehaviour
{
    //[SerializeField] private GameObject levelTerrainObject; //Level Terrain GameObject
    [SerializeField] private Terrain levelTerrain;
    private float terrainSizeX;
    private float terrainSizeZ;

    [SerializeField] private GameObject playerBase;
    [SerializeField] private GameObject enemyBase;
    [SerializeField] private GameObject resource1; //First resource - eg. Tree
    [SerializeField] private GameObject resource2; //First resource - eg. Iron Mine

    [SerializeField] private int resourceSpacing; //Spacing between resources
    [SerializeField] private float resourceSpawnShift; //Amount of random shift from original resource spawn
    [SerializeField] private float baseDistance; //Minimum empty distance around bases

    private Vector3 playerBaseLocation; //Location in world of Player Base;
    private Vector3 enemyBaseLocation; //Location in world of Enemy Base;

    // Start is called before the first frame update
    void Start()
    {
        terrainSizeX = levelTerrain.terrainData.size.x;
        terrainSizeZ = levelTerrain.terrainData.size.z;

        SpawnPlayerBase(); //Spawn Player Base
        SpawnEnemyBase(); //Spawn Enemy Base taking note of Player Base location
        SpawnResources(); //Spawn Resources

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnPlayerBase()
    {
        GameObject instantiatedBase = Instantiate(playerBase, GenerateRandomSpawn(), Quaternion.identity);
        playerBaseLocation = instantiatedBase.transform.position;
    }

    private void SpawnEnemyBase()
    {
        Vector3 spawnLocation;
        do
        {
            spawnLocation = GenerateRandomSpawn();
        } while (Vector3.Distance(spawnLocation,playerBaseLocation)<baseDistance); //Check that spawn location is not too close to enemy base

        GameObject instantiatedBase = Instantiate(enemyBase, spawnLocation, Quaternion.identity);
        enemyBaseLocation = instantiatedBase.transform.position;
    }

    private void SpawnResources()
    {
        
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
                    int randomNum = Random.Range(0, 10); //Random number to decide what resource
                    if (randomNum == 0)
                    {
                        Instantiate(resource2, resourcePosition, Quaternion.identity); // 1 in 10 chance of instantating rarer resource
                    }
                    else Instantiate(resource1, resourcePosition, Quaternion.identity); // 9 in 10 chance of instantiating common resource
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

    private Vector3 GenerateRandomSpawn()
    {
        Vector3 spawnLocation = new Vector3(Random.Range(0,terrainSizeX),
            0, //Note: Spawn Location Y may be changed in case of shaped terrain, in which case obtain Y using Terrain.sampleHeight after first generation
            Random.Range(0, terrainSizeZ));

        return spawnLocation;
    }

}
