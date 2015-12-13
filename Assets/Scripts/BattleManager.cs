﻿using UnityEngine;
using System.Collections;

public class BattleManager : MonoBehaviour {

    public float scalingFactor;

    public GameObject wall;
    public GameObject flag;
    public GameObject flagAdjacent;
    public GameObject tower;
    public GameObject building;
    public GameObject gate;

    public GameObject player;

    public GameObject fort;
    public GameObject defenders;
    public GameObject attackers;

    private int[,] heightmap;
    private Battle battle;

	// Use this for initialization
	void Start () {
        battle = new Battle(JSONReader.ConvertJSON("ExampleJSON"));
        heightmap = new int[battle.width, battle.height];
        Build(battle);
        CreateHeightmap(battle);
        PlacePlayers(battle);
	}

    /// <summary>
    /// Builds the battle components
    /// </summary>
    /// <param name="b">Battle to build</param>
    private void Build(Battle b)
    {
        foreach (Sector s in battle.sectors)
        {
            switch (s.sectorType)
            {
                case SectorType.Wall:
                    BuildWall(s);
                    break;
                case SectorType.Tower:
                    BuildTower(s);
                    break;
                case SectorType.Gate:
                    BuildGate(s);
                    break;
                case SectorType.Flag:
                    BuildFlag(s);
                    break;
                case SectorType.FlagAdjacent:
                    BuildFlagAdjacent(s);
                    break;
                case SectorType.Building:
                    BuildBuilding(s);
                    break;
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// Builds a wall
    /// </summary>
    /// <param name="s">Sector to build</param>
    private void BuildWall(Sector s)
    {
        GameObject go = new GameObject();
        go.transform.parent = fort.transform;
        go.name = "Wall";

        foreach(Vector2 tile in s.tiles)
        {
            GameObject w = Instantiate(wall, new Vector3(battle.width / 2f - tile.x - 0.5f, 0.5f, tile.y-battle.height/2+0.5f), Quaternion.identity) as GameObject;
            if (w.GetComponent<WallObject>() != null)
                w.GetComponent<WallObject>().SetHeight(s.height*scalingFactor);

            if (s.orientation == Orientation.Vertical)
                w.transform.localRotation = Quaternion.Euler(0, 90, 0);

            w.transform.parent = go.transform;
        }
    }

    /// <summary>
    /// Builds a tower
    /// </summary>
    /// <param name="s">Sector to build</param>
    private void BuildTower(Sector s)
    {

        Vector2 averagePosition = new Vector2();
        foreach (Vector2 tile in s.tiles)
        {
            averagePosition += tile;
        }

        averagePosition /= s.tiles.Count;

        int towerSize = Mathf.RoundToInt(Mathf.Sqrt(s.tiles.Count));
        GameObject t = Instantiate(tower, new Vector3(battle.width / 2f - averagePosition.x - 0.5f, 0.5f, averagePosition.y - battle.height / 2 + 0.5f), Quaternion.identity) as GameObject;
        if (t.GetComponent<TowerObject>() != null)
            t.GetComponent<TowerObject>().SetSize(s.height * scalingFactor, towerSize);

        t.transform.parent = fort.transform;
    }

    /// <summary>
    /// Builds a gate
    /// </summary>
    /// <param name="s">Sector to build</param>
    private void BuildGate(Sector s)
    {
        GameObject go = new GameObject();
        go.transform.parent = fort.transform;
        go.name = "Gate";

        foreach (Vector2 tile in s.tiles)
        {
            GameObject w = Instantiate(gate, new Vector3(battle.width / 2f - tile.x - 0.5f, 0.5f, tile.y - battle.height / 2 + 0.5f), Quaternion.identity) as GameObject;
            if (w.GetComponent<GateObject>() != null)
                w.GetComponent<GateObject>().SetHeight(GetWallHeight(battle) * scalingFactor, scalingFactor);

            w.transform.parent = go.transform;
        }
    }

    /// <summary>
    /// Builds a flag in a specified sector
    /// </summary>
    /// <param name="s">Sector to build a flag on</param>
    private void BuildFlag(Sector s)
    {
        GameObject go = new GameObject();
        go.transform.parent = fort.transform;
        go.name = "Flag";

        foreach (Vector2 tile in s.tiles)
        {
            GameObject f = Instantiate(flag, new Vector3(battle.width / 2f - tile.x - 0.5f, 0.5f, tile.y - battle.height / 2 + 0.5f), Quaternion.identity) as GameObject;
            if (f.GetComponent<FlagObject>() != null)
                f.GetComponent<FlagObject>().SetHeight(s.height * scalingFactor);

            f.transform.parent = go.transform;
        }
    }

    /// <summary>
    /// Sector adjacent to the flag
    /// </summary>
    /// <param name="s">Sector to build</param>
    private void BuildFlagAdjacent(Sector s)
    {
        GameObject go = new GameObject();
        go.transform.parent = fort.transform;
        go.name = "Flag Adjacent";

        foreach (Vector2 tile in s.tiles)
        {
            GameObject f = Instantiate(flagAdjacent, new Vector3(battle.width / 2f - tile.x - 0.5f, 0.5f, tile.y - battle.height / 2 + 0.5f), Quaternion.identity) as GameObject;
            if (f.GetComponent<FlagAdjacentObject>() != null)
                f.GetComponent<FlagAdjacentObject>().SetHeight(s.height * scalingFactor);

            f.transform.parent = go.transform;
        }
    }

    /// <summary>
    /// Builds a building in the sector
    /// </summary>
    /// <param name="s">Sector to build</param>
    private void BuildBuilding(Sector s)
    {
        Vector2 averagePosition = new Vector2();
        foreach (Vector2 tile in s.tiles)
        {
            averagePosition += tile;
        }

        averagePosition /= s.tiles.Count;
        Vector2 buildingSize = GetSize(s);

        GameObject b = Instantiate(building, new Vector3(battle.width / 2f - averagePosition.x - 0.5f, 0.5f, averagePosition.y - battle.height / 2 + 0.5f), Quaternion.identity) as GameObject;
        if (b.GetComponent<BuildingObject>() != null)
            b.GetComponent<BuildingObject>().SetSize(s.height * scalingFactor, buildingSize);

        b.transform.parent = fort.transform;
    }

    /// <summary>
    /// Places the players on the battlefield
    /// </summary>
    /// <param name="b">Battle to put the players on</param>
    private void PlacePlayers(Battle b)
    {
        foreach(Player p in b.attackers)
        {
            GameObject player = Instantiate(this.player, new Vector3(battle.width/2f - p.startpos.x - 0.5f, GetHeight(p.startpos)*scalingFactor+0.5f, p.startpos.y - battle.height/2f + 0.5f), Quaternion.identity) as GameObject;
            if (player.GetComponent<PlayerObject>() != null)
                player.GetComponent<PlayerObject>().SetSide(Side.Attack);
            player.transform.parent = attackers.transform;
        }

        foreach (Player p in b.defenders)
        {
            GameObject player = Instantiate(this.player, new Vector3(battle.width / 2f - p.startpos.x - 0.5f, GetHeight(p.startpos) * scalingFactor + 0.5f, p.startpos.y - battle.height / 2f + 0.5f), Quaternion.identity) as GameObject;
            if (player.GetComponent<PlayerObject>() != null)
            {
                player.GetComponent<PlayerObject>().SetSide(Side.Defense);
            }
            player.transform.parent = defenders.transform;
        }
    }
	
    /// <summary>
    /// Get the wall height in the battle, needed for the gate
    /// </summary>
    /// <param name="b">Current battle</param>
    /// <returns>float describing the wall height</returns>
    private float GetWallHeight(Battle b)
    {
        foreach(Sector s in b.sectors)
        {
            if (s.sectorType == SectorType.Wall)
                return s.height;
        }

        return 0;
    }
    /// <summary>
    /// Returns the width and height of a sector
    /// </summary>
    /// <param name="s">Sector to check</param>
    /// <returns></returns>
    private Vector2 GetSize(Sector s)
    {
        Vector2 result = new Vector2();
        foreach(Vector2 tile in s.tiles)
        {
            foreach(Vector2 otherTile in s.tiles)
            {
                result.x = Mathf.Max(result.x, otherTile.x - tile.x+1);
                result.y = Mathf.Max(result.y, otherTile.y - tile.y+1);
            }
        }
        return result;
    }

    /// <summary>
    /// Creates a heightmap for the battle
    /// </summary>
    /// <param name="b">Battle to build a heightmap for</param>
    private void CreateHeightmap(Battle b)
    {
        foreach(Sector s in b.sectors)
        {
            foreach(Vector2 tile in s.tiles)
            {
                heightmap[Mathf.RoundToInt(tile.x), Mathf.RoundToInt(tile.y)] = s.height;
            }
        }
    }

    private int GetHeight(Vector2 position)
    {
        return heightmap[Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)];
    }
}