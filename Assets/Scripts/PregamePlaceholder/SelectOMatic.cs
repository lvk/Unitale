using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SelectOMatic : MonoBehaviour
{
    public Button b;
    public SelectionTarget target;

    public enum SelectionTarget
    {
        MODFOLDER,
        ENCOUNTER
    };

    // Use this for initialization
    private void Start()
    {
        if (target == SelectionTarget.MODFOLDER)
        {
            modFolderSelection();
        }
        else if (target == SelectionTarget.ENCOUNTER)
        {
            encounterSelection();
        }
    }

    /// <summary>
    /// Makes the buttons for the mod selection screen.
    /// </summary>
    private void modFolderSelection()
    {
        DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(FileLoader.DataRoot, "Mods"));
        DirectoryInfo[] modDirs = di.GetDirectories();
        int numButton = 0;
        foreach (DirectoryInfo modDir in modDirs)
        {
            Button c = Instantiate(b);
            c.transform.SetParent(GameObject.Find("Content").transform);
            RectTransform crt = c.GetComponent<RectTransform>();
            crt.anchoredPosition = new Vector2(10, 0 - 40 * numButton);
            c.GetComponentInChildren<Text>().text = modDir.Name;
            string mdn = modDir.Name; // create a new object in memory because the reference to moddir in the anonymous function gets fucked
            c.onClick.AddListener(() => { StaticInits.MODFOLDER = mdn; Debug.Log("Selecting directory " + mdn); Application.LoadLevel("EncounterSelect"); });
            numButton++;
        }
    }

    /// <summary>
    /// Makes the buttons for the encounter selection screen. Code duplication? I don't know what you're talking about.
    /// </summary>
    private void encounterSelection()
    {
        DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(FileLoader.DataRoot, "Mods/" + StaticInits.MODFOLDER + "/Lua/Encounters"));
        FileInfo[] encounterFiles = di.GetFiles();
        int numButton = 0;

        Button back = Instantiate(b);
        back.transform.SetParent(GameObject.Find("Canvas").transform);
        back.GetComponent<RectTransform>().anchoredPosition = new Vector2(320, -40);
        back.GetComponentInChildren<Text>().text = "Back";
        back.onClick.AddListener(() => { Application.LoadLevel("ModSelect"); });

        foreach (FileInfo encounterFile in encounterFiles)
        {
            if (!encounterFile.Name.EndsWith(".lua"))
                continue;

            Button c = Instantiate(b);
            c.transform.SetParent(GameObject.Find("Content").transform);
            RectTransform crt = c.GetComponent<RectTransform>();
            crt.anchoredPosition = new Vector2(10, 0 - 40 * numButton);
            c.GetComponentInChildren<Text>().text = encounterFile.Name;
            string efn = Path.GetFileNameWithoutExtension(encounterFile.Name); // create a new object in memory because the reference to moddir in the anonymous function gets fucked
            c.onClick.AddListener(() => { StaticInits.ENCOUNTER = efn; Debug.Log("Loading " + efn); Application.LoadLevel("Battle"); });
            numButton++;
        }
    }
}