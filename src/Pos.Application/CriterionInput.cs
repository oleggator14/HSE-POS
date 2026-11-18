namespace Pos.Application;

// Тип передаваемых данных для SetupRubric
public sealed record CriterionInput(string Name, int WeightPercent, int MaxScore);
