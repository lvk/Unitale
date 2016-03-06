/// <summary>
/// Class for ingame items. Currently just creates TestDog# items.
/// </summary>
public class UnderItem
{
    private static int dogNumber = 1;

    public UnderItem()
    {
        ID = "DOGTEST" + dogNumber;
        ShortName = "TestDog" + dogNumber;
        dogNumber++;
    }

    public string ID { get; private set; }
    public string ShortName { get; private set; }

    public void inOverworldUse()
    {
    }

    public void inCombatUse()
    {
        UIController.instance.ActionDialogResult(new RegularMessage("It's the default handler!\nNot as cool as you hoped."), UIController.UIState.ENEMYDIALOGUE);
    }
}