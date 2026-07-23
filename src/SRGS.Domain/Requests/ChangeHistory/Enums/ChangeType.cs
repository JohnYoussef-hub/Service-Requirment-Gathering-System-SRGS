namespace SRGS.Domain.Requests.History.Enums;

// Backed by CHECK CK_CHANGE_type. Nullable in the DB, so this enum only covers the
// "has a value" states — keep the column nullable in the entity too.
public enum ChangeType
{
    RuleSet,
    Model,
    FormDesigner,
    Workflow,
    Script,
    CatalogItem,
    Integration
}
