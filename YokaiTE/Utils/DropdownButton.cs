namespace YokaiTE.Utils;

public class DropdownButton
{
    public string Icon { get; set; }
    public string Text { get; set; }
    public Func<Task> OnClick { get; set; }
}