using Eto.Drawing;

namespace NAPS2.EtoForms;

public class DefaultIconProvider : IIconProvider
{
    public Image? GetIcon(string name, bool oversized = false)
    {
        var data = (byte[]?) Icons.ResourceManager.GetObject(name);
        if (data == null) return null;
        return new Bitmap(data);
    }
}