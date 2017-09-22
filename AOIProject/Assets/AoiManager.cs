using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoiManager : MonoBehaviour {
    
    // 所有格子 all grids
    public Dictionary<string, Grid> allGrids = new Dictionary<string, Grid>(100);

    // 所有的实体玩家 all entity
    const int entityCount = 100;
    Entity[] allEntity = new Entity[entityCount];
    
    // 控制的玩家 the player of control
    Entity playerEntity;
    
    // 视野范围 单位是unity里的单位 player field (real unit in unity)
    public const float FIELD = 10;

    const float randomRange = 100;

	// Use this for initialization
	void Start () {
        //生成100实体并初始化第一个为主角控制 create 100 player and instantial player of control
        Material mat = Resources.Load<Material>("Cube");
        for (int i = 0; i < entityCount; i++)
        {
            Random.InitState(i);
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = i.ToString();
            go.transform.position = new Vector3(Random.Range(-randomRange, randomRange), 0, Random.Range(-randomRange, randomRange));
            go.transform.localScale = new Vector3(3, 3, 3);
            go.GetComponent<BoxCollider>().enabled = false;
            MeshRenderer render = go.GetComponent<MeshRenderer>();
            render.material = mat;
            if (i == 0)
            {
                render.material.color = Color.green;
            }
            else
            {
                render.material.color = Color.white;
            }

            Entity entity = new Entity();
            entity.go = go;
            entity.mat = render.material;
            if (i == 0)
            {
                playerEntity = entity;
                entity.isPlayer = true;
            }
            allEntity[i] = entity;

            RefreshAoi(entity);
        }
        
        
    }

    int playerIndex;
	// Update is called once per frame
	void Update () {
        //切换实体 测试用 change control entity for test
        if (Input.GetKeyDown(KeyCode.Q))
        {
            playerEntity.isPlayer = false;
            playerEntity.mat.color = Color.white;

            playerIndex++;
            if (playerIndex >= entityCount)
            {
                playerIndex = 0;
            }
            playerEntity = allEntity[playerIndex];

            playerEntity.mat.color = Color.green;
            playerEntity.isPlayer = true;

            foreach (var item in allEntity)
            {
                if (item != playerEntity)
                {
                    item.mat.color = Color.white;
                }
            }
        }

        if (Input.GetKey(KeyCode.W))
        {
            playerEntity.go.transform.position += new Vector3(0, 0, 0.5f);
            RefreshAoi(playerEntity);
        }
        if (Input.GetKey(KeyCode.S))
        {
            playerEntity.go.transform.position += new Vector3(0, 0, -0.5f);
            RefreshAoi(playerEntity);
        }
        if (Input.GetKey(KeyCode.A))
        {
            playerEntity.go.transform.position += new Vector3(-0.5f, 0, 0);
            RefreshAoi(playerEntity);
        }
        if (Input.GetKey(KeyCode.D))
        {
            playerEntity.go.transform.position += new Vector3(0.5f, 0, 0);
            RefreshAoi(playerEntity);
        }

        for (int i = -100; i <= 99; i++)
        {
            if (i % AoiManager.FIELD == 0)
            {
                float space = AoiManager.FIELD / 2;
                Debug.DrawLine(new Vector3(i + space, 0, -100), new Vector3(i + space, 0, 100), Color.blue);
                Debug.DrawLine(new Vector3(-100, 0, i + space), new Vector3(100, 0, i + space), Color.blue);
            }
        }

        //让除了主角的所有物体行动 测试用
        for (int i = 0; i < allEntity.Length; i++)
        {
            if (allEntity[i] != playerEntity)
            {
                if (i % 2 == 0)
                {
                    allEntity[i].go.transform.position += new Vector3(allEntity[i].forward, 0, 0);
                    if (allEntity[i].go.transform.position.x >= 99 || allEntity[i].go.transform.position.x <= -99)
                    {
                        allEntity[i].forward = -allEntity[i].forward;
                    }
                    RefreshAoi(allEntity[i]);
                }
                else
                {
                    allEntity[i].go.transform.position += new Vector3(0, 0, allEntity[i].forward);
                    if (allEntity[i].go.transform.position.z >= 99 || allEntity[i].go.transform.position.z <= -99)
                    {
                        allEntity[i].forward = -allEntity[i].forward;
                    }
                    RefreshAoi(allEntity[i]);
                }
            }
        }

    }


    /// <summary>
    /// 刷新视野 refresh aoi
    /// </summary>
    /// <param name="entity"></param>
    void RefreshAoi(Entity entity)
    {
        int x = 0;
        int z = 0;
        float space = AoiManager.FIELD / 2;
        if (entity.go.transform.position.x >= 0)
        {
            x = (int)((entity.go.transform.position.x + space) / AoiManager.FIELD);
        }
        else
        {
            x = (int)((entity.go.transform.position.x - space) / AoiManager.FIELD);
        }
        if (entity.go.transform.position.z >= 0)
        {
            z = (int)((entity.go.transform.position.z + space) / AoiManager.FIELD);
        }
        else
        {
            z = (int)((entity.go.transform.position.z - space) / AoiManager.FIELD);
        }

        string key = string.Format("{0},{1}", x, z);
        if (entity.gridKey != key)
        {
            if (allGrids.ContainsKey(entity.gridKey))
            {
                allGrids[entity.gridKey].OutGrid(entity);
            }

            if (!allGrids.ContainsKey(key))
            {
                allGrids.Add(key, new Grid(key, allGrids));
            }

            allGrids[key].EnterGrid(entity);

        }
    }


}

public class Entity
{
    public string gridKey = "";
    public int x;
    public int z;

    public GameObject go;
    public Material mat;


    public HashSet<Grid> lastInterestGridList = new HashSet<Grid>();
    public HashSet<Grid> currentInterestGridList = new HashSet<Grid>();

    public HashSet<Entity> currentInterestEntityList = new HashSet<Entity>();
    
    // 测试用自动的移动速度 test speed for move entity
    public float forward = 0.5f;

    //是否是主角 is player of control
    public bool isPlayer;

    /// <summary>
    /// 当感兴趣的格子有玩家进入 when some one enter interest grid
    /// </summary>
    /// <param name="entity"></param>
    public void OnOtherEnterGrid(Entity entity)
    {
        if (entity != this)
        {
            currentInterestEntityList.Add(entity);
            if (isPlayer)
            {
                entity.mat.color = Color.red;
            }
        }
    }

    /// <summary>
    /// 当感兴趣的格子有玩家离开 when some one out interest grid
    /// </summary>
    /// <param name="entity"></param>
    public void OnOtherOutGrid(Entity entity)
    {
        if (entity != this)
        {
            currentInterestEntityList.Remove(entity);
            if (isPlayer)
            {
                entity.mat.color = Color.white;
            }
        }
    }

    /// <summary>
    /// 关注某格子 when interest some gird
    /// </summary>
    /// <param name="entity"></param>
    public void OnInterestGrid(Grid grid)
    {
        currentInterestGridList.Add(grid);
        foreach (var item in grid.existEntityList)
        {
            currentInterestEntityList.Add(item);
            if (isPlayer)
            {
                if (item != this)
                {
                    item.mat.color = Color.red;
                }
            }
        }
    }

    /// <summary>
    /// 不再关注某格子 when no interest some grid
    /// </summary>
    /// <param name="entity"></param>
    public void OnNonInterestGrid(Grid grid)
    {
        currentInterestGridList.Remove(grid);
        currentInterestEntityList.ExceptWith(grid.existEntityList);
        foreach (var item in grid.existEntityList)
        {
            if (isPlayer)
            {
                if (item != this)
                {
                    item.mat.color = Color.white;
                }
            }
        }
    }

}

public class Grid
{
    //格子所属的总的列表
    public Dictionary<string, Grid> allGrids;
    public string gridKey;
    public HashSet<Entity> existEntityList = new HashSet<Entity>();
    public HashSet<Entity> interestMeList = new HashSet<Entity>();

    public Grid(string key, Dictionary<string, Grid> allGrids)
    {
        this.gridKey = key;
        this.allGrids = allGrids;
    }

    /// <summary>
    /// 进入格子 enter some grid
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="allGrids"></param>
    public void EnterGrid(Entity entity)
    {
        //如果添加格子里的玩家列表成功
        if (existEntityList.Add(entity))
        {
            //通知对这个格子感兴趣的人
            foreach (var item in interestMeList)
            {
                item.OnOtherEnterGrid(entity);
            }

            int x = 0;
            int z = 0;
            float space = AoiManager.FIELD / 2;
            if (entity.go.transform.position.x >= 0)
            {
                x = (int)((entity.go.transform.position.x + space) / AoiManager.FIELD);
            }
            else
            {
                x = (int)((entity.go.transform.position.x - space) / AoiManager.FIELD);
            }
            if (entity.go.transform.position.z >= 0)
            {
                z = (int)((entity.go.transform.position.z + space) / AoiManager.FIELD);
            }
            else
            {
                z = (int)((entity.go.transform.position.z - space) / AoiManager.FIELD);
            }

            string key = string.Format("{0},{1}", x, z);

            //更新玩家格子信息 refresh entity grid info
            entity.gridKey = key;
            entity.x = x;
            entity.z = z;

            //增加对周围9个格子的兴趣 add 9 grid around the sentity
            for (int i = x - 1; i <= x + 1; i++)
            {
                for (int j = z - 1; j <= z + 1; j++)
                {
                    key = string.Format("{0},{1}", i, j);
                    if (!allGrids.ContainsKey(key))
                    {
                        allGrids.Add(key, new Grid(key, allGrids));
                    }
                    allGrids[key].AddInterester(entity);
                }
            }
            //检查不在感兴趣的格子 check no interest grid
            entity.lastInterestGridList.ExceptWith(entity.currentInterestGridList);
            foreach (var item in entity.lastInterestGridList)
            {
                entity.OnNonInterestGrid(item);
            }

        }
    }

    /// <summary>
    /// 离开格子 out some grid
    /// </summary>
    /// <param name="entity"></param>
    public void OutGrid(Entity entity)
    {
        //如果成功从格子列表里删除 if success remove from exist entity list
        if (existEntityList.Remove(entity))
        {
            entity.gridKey = "";
            //清空上一次记录的感兴趣的人 clear last record interest entity
            entity.lastInterestGridList.Clear();

            foreach (var item in entity.currentInterestGridList)
            {
                entity.lastInterestGridList.Add(item);
                item.RemoveInterester(entity);
            }
            entity.currentInterestGridList.Clear();

            //通知感兴趣的人有人离开格子了 notify all interest player someone out grid
            foreach (var item in interestMeList)
            {
                item.OnOtherOutGrid(entity);
            }

        }
    }

    /// <summary>
    /// 增加对这个格子感兴趣的人 add someone interest this grid
    /// </summary>
    /// <param name="entity"></param>
    public void AddInterester(Entity entity)
    {
        //如果对这个格子感兴趣成功
        if (interestMeList.Add(entity))
        {
            entity.OnInterestGrid(this);
        }
    }

    /// <summary>
    /// 移除对这个格子感兴趣的人 remove someone interest this grid
    /// </summary>
    /// <param name="entity"></param>
    public void RemoveInterester(Entity entity)
    {
        if (interestMeList.Remove(entity))
        {
            //发送该地图所有的玩家信息给这个感兴趣的人 notify all interest player the players in this grid is remove 
            foreach (var item in existEntityList)
            {
                entity.OnOtherOutGrid(item);
            }
        }
        //如果没人感兴趣 则从格子列表中删除 if no one interest this grid, remove from all grids
        if (interestMeList.Count == 0)
        {
            allGrids.Remove(gridKey);
        }
    }

}
