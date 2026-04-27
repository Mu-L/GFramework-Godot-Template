namespace GFrameworkGodotTemplate.scripts.config;

public interface ITemplateContentCatalog : IUtility
{
    MenuTextConfig GetMenuText();

    string GetCurrentLanguageId();

    void Reload();
}
