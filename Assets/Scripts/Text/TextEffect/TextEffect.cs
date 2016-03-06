public abstract class TextEffect
{
    private bool doUpdate = true;
    protected TextManager textMan;

    public TextEffect(TextManager textMan)
    {
        this.textMan = textMan;
    }

    public void updateEffects()
    {
        /*doUpdate = !doUpdate; // use boolean to halve the amount of update calls, saving cpu cycles

        if (doUpdate)
            updateInternal();*/
        updateInternal();
    }

    protected abstract void updateInternal();
}