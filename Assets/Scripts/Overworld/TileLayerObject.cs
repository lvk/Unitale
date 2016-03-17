using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TiledSharp;
using System;

public class TileLayerObject : MonoBehaviour {

    public TmxLayer myLayer;

    List<LayerPart> layerParts = new List<LayerPart>();

    public void GenerateMeshes() {
        layerParts.Clear();

        int index = 0;

        LayerPart.tileCol=(GameObject)Resources.Load("Overworld/TileCollision");

        foreach (TmxTileset t in OverworldController.loadedMap.Tilesets) {
            GameObject newObject = new GameObject();
            LayerPart lp = new LayerPart();

            lp.obj=newObject;
            lp.filter = newObject.AddComponent<MeshFilter>();
            lp.renderer = newObject.AddComponent<MeshRenderer>();
            lp.coll=newObject.AddComponent<PolygonCollider2D>();
            newObject.AddComponent<SpriteDepthSorting>();
            lp.set=t;
            lp.UpdateRenderer();

            newObject.name=t.Name;
            newObject.transform.SetParent(transform);
            newObject.transform.localPosition=Vector3.zero;

            layerParts.Add(lp);
            index++;
        }

        if (myLayer.Properties.ContainsKey("visible")) {
            if (myLayer.Properties["visible"]=="false")
                foreach (LayerPart p in layerParts)
                    p.renderer.enabled=false;
        }

        foreach (TmxLayerTile t in myLayer.Tiles)
            GenerateTile(t);

        foreach(LayerPart p in layerParts) {
            p.UpdateMesh();
        }

        if (myLayer.Properties.ContainsKey("enabled")) {
            bool set = bool.Parse(myLayer.Properties["enabled"]);
        }
    }

    void CheckDictionaries() {
        foreach (LayerPart p in layerParts)
            Destroy(p.obj);
    }

    void GenerateTile(TmxLayerTile t) {
        if (t.Gid==0)
            return;

        AddFace(OverworldController.GetTilesetForID(t.Gid),t);
    }

    public void AddFace(int layerID, TmxLayerTile tile) {
        LayerPart l = layerParts[layerID];

        l.AddCollision(layerID,tile);

        l.AddFace(tile.Gid,tile.X, OverworldController.loadedMap.Height-tile.Y);
    }
}

public class LayerPart {
    public GameObject obj;
    public MeshFilter filter;
    public MeshRenderer renderer;
    public PolygonCollider2D coll;
    public Mesh mesh;
    public TmxTileset set;

    public static GameObject tileCol;

    public List<Vector3> vertices = new List<Vector3>();
    public List<Vector2> uv = new List<Vector2>();
    public List<Vector2> collisionData = new List<Vector2>();
    public List<int> triangles = new List<int>();

    int faceCount = 0;

    int tileHeight = 0;
    int tileWidth = 0;

    public void UpdateRenderer() {

        byte[] data = System.IO.File.ReadAllBytes(Application.dataPath+"/TILED/Tilesets/"+System.IO.Path.GetFileNameWithoutExtension(set.Image.Source)+".png");

        Texture2D t = new Texture2D(5, 5);
        t.LoadImage(data);
        t.filterMode=FilterMode.Point;

        renderer.material=Resources.Load<Material>("Overworld/TileLayerMaterial");
        renderer.material.mainTexture=t;

        tileWidth=(int)set.Image.Width/set.TileWidth;
        tileHeight=(int)set.Image.Height/set.TileHeight;
    }

    public void UpdateMesh() {
        if (mesh==null)
            mesh=new Mesh();
        mesh.Clear();

        mesh.vertices=vertices.ToArray();
        mesh.uv=uv.ToArray();
        mesh.triangles=triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.Optimize();

        coll.points=collisionData.ToArray();
        collisionData.Clear();

        faceCount=0;

        filter.mesh=mesh;
    }

    public void AddFace(int tileID, int x, int y) {
        vertices.Add(new Vector3(x, y, 0));
        vertices.Add(new Vector3(x+1, y, 0));
        vertices.Add(new Vector3(x+1, y-1, 0));
        vertices.Add(new Vector3(x, y-1, 0));

        triangles.Add(faceCount*4);
        triangles.Add((faceCount*4)+1);
        triangles.Add((faceCount*4)+3);
        triangles.Add((faceCount*4)+1);
        triangles.Add((faceCount*4)+2);
        triangles.Add((faceCount*4)+3);

        uv.AddRange(GetUVForTileID(tileID));

        faceCount++;
    }

    public Vector2[] GetUVForTileID(int tileID) {
        Vector2[] returnValue = new Vector2[4];

        int realID = tileID-set.FirstGid;

        if (realID<0)
            return returnValue;

        int x = 0;
        int y = 0;

        for (int i = 0; i<tileID; i++) {
            if (x>=tileWidth) {
                y++;
                x=0;
            }
            x++;
        }


        float xCoord = (20f*(x-1))/(float)set.Image.Width;
        float yCoord = (20f*(y+1))/(float)set.Image.Height;
        float xSize = 20/(float)set.Image.Width;
        float ySize = 20/(float)set.Image.Height;

        returnValue[3]=new Vector2(xCoord*1.001f, (1-yCoord)*1.001f);
        returnValue[0]=new Vector2(xCoord*1.001f, (1-yCoord+ySize)*0.999f);
        returnValue[1]=new Vector2((xCoord+xSize)*0.999f, (1-yCoord+ySize)*0.999f);
        returnValue[2]=new Vector2((xCoord+xSize)*0.999f, (1-yCoord)*1.001f);

        return returnValue;
    }

    internal void AddCollision(int layerID, TmxLayerTile tile) {
        TmxTileset set = OverworldController.loadedMap.Tilesets[layerID];

        int realID = tile.Gid-set.FirstGid;

        if (realID<=0)
            return;

        TmxTilesetTile getTile = null;

        foreach (TmxTilesetTile t in set.Tiles) {
            if (t.Id==realID) {
                getTile=t;
            }
        }

        if (getTile==null)
            return;
        if(getTile.Properties.ContainsKey("boxcollider")){
            GameObject newCol = GameObject.Instantiate(tileCol);

            newCol.transform.position=new Vector3(tile.X,OverworldController.loadedMap.Height-tile.Y,0)+new Vector3(0.5f,-0.5f,0);
            newCol.transform.SetParent(obj.transform);
        }
    }
}
