using Pos.Domain.Rubrics;

namespace Pos.Domain.Ports;

// Рубрикатор в системе один, поэтому без идентификатора

public interface IRubricRepository
{
    Rubric? Get();
    void Save(Rubric rubric);
}
