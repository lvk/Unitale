﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TiledSharp;
using System.IO;
using System;

public class OverworldController : MonoBehaviour {

    public static TmxMap loadedMap;
    static OverworldController main;
    Transform objects;
    GameObject playerPrefab, characterPrefab, objectPrefab;

    public string LevelName;

    GameObject layerPrefab;
    List<TileLayerObject> layerObjects = new List<TileLayerObject>();

    public static bool isOverworld = false;

	// Use this for initialization
	void Start () {
        main=this;
        isOverworld=true;

        layerPrefab=Resources.Load<GameObject>("Overworld/TileLayer");
        objects=transform.FindChild("Objects");
        playerPrefab=Resources.Load<GameObject>("Overworld/Player");
        characterPrefab=Resources.Load<GameObject>("Overworld/Character");
        objectPrefab=Resources.Load<GameObject>("Overworld/Object");

        ScriptRegistry.init();

        AddCustomObjectProperties();

        LoadLevel(LevelName);
	}

    void Update() {
        if (Input.GetKeyDown(KeyCode.G))
            EncounterEnemy();
    }

    public static void LoadLevel(string filename) {
        string path = FileLoader.ModDataPath+"/Maps/"+filename+".tmx";

        loadedMap=new TmxMap(path);

        main.OnWorldLoaded();
    }

    void OnWorldLoaded() {
        CreateNewLayers();
        GenerateLayers();
        GenerateObjects();
    }

    void CreateNewLayers() {
        int index = 0;
        while (layerObjects.Count<loadedMap.Layers.Count) {
            GameObject newLayerObject = Instantiate(layerPrefab);
            newLayerObject.transform.SetParent(transform);
            newLayerObject.transform.localPosition=new Vector3(0,0,-(float)index/10);
            layerObjects.Add(newLayerObject.GetComponent<TileLayerObject>());
            index++;
        }
    }

    void GenerateLayers() {
        int index = 0;
        foreach(TmxLayer l in loadedMap.Layers) {

            layerObjects[index].myLayer=l;
            layerObjects[index].GenerateMeshes();

            index++;
        }
    }

    void GenerateObjects() {
        foreach(TmxObjectGroup objGroup in loadedMap.ObjectGroups) {
            foreach(TmxObjectGroup.TmxObject obj in objGroup.Objects) {

                if (!obj.Properties.ContainsKey("type"))
                    continue;

                string type = obj.Properties["type"];

                if (type=="character"||type=="Character") {
                    GenerateCharacterObject(obj);
                } else if (type=="player"||type=="Player") {
                    GeneratePlayerObject(obj);
                } else {
                    GenerateObject(obj);
                }
                    
            }
        }
    }

    private void GenerateObject(TmxObjectGroup.TmxObject obj) {
        GameObject newObject = Instantiate(objectPrefab);

        newObject.transform.position=new Vector3((float)obj.X/loadedMap.TileWidth+0.5f, loadedMap.Height-((float)obj.Y/loadedMap.TileHeight)+0.5f, 0);
        newObject.transform.SetParent(objects);

        newObject.GetComponent<LuaOverworldObject>().UpdateToObject(obj);
    }

    private void GeneratePlayerObject(TmxObjectGroup.TmxObject obj) {
        GameObject newObject = Instantiate(playerPrefab);

        newObject.name=obj.Name;

        newObject.transform.position=new Vector3((float)obj.X/loadedMap.TileWidth+0.5f, loadedMap.Height-((float)obj.Y/loadedMap.TileHeight)+0.5f, 0);
        newObject.transform.SetParent(objects);

        newObject.GetComponent<OverworldCharacterController>().UpdateToObject(obj);
    }

    private void GenerateCharacterObject(TmxObjectGroup.TmxObject obj) {
        GameObject newObject = Instantiate(characterPrefab);

        newObject.name=obj.Name;

        newObject.transform.position=new Vector3((float)obj.X/loadedMap.TileWidth+0.5f, loadedMap.Height-((float)obj.Y/loadedMap.TileHeight)+0.5f, 0);
        newObject.transform.SetParent(objects);

        newObject.GetComponent<OverworldCharacterController>().UpdateToObject(obj);
    }

    public static int GetTilesetForID(int id) {
        int rId = -1;
        int index = 0;
        foreach (TmxTileset t in loadedMap.Tilesets) {
            if (id>=t.FirstGid)
                rId=index;
            index++;
        }

        return rId;
    }


    private void AddCustomObjectProperties() {
        LuaOverworldObject.avaliableProperties.Add("interactable",typeof(Interactable));
        LuaOverworldObject.avaliableProperties.Add("dialogprovider",typeof(DialogProvider));
    }


    public static void EncounterEnemy() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(4);
        StaticInits.isOverworld=false;
        StaticInits.MODFOLDER="OverworldExample";
        StaticInits.ENCOUNTER="encounter";
    }
}
