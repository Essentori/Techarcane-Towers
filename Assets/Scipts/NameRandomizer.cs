using UnityEngine;

public class NameRandomizer : MonoBehaviour
{
    private int _globalRevision = 0;
    private int _lettersIndex = 0;
    private string[] _prefixes = { "Prototype", "Construct", "Platform", "Specification", "Artifact", "Node", "Array" };

    public void RandomizeConstructionName(IBuildable construction)
    {
        string prefix = string.Empty;
        if (Random.value < 0.6f)
        {
            prefix = _prefixes[Random.Range(0, _prefixes.Length)] + ": "; 
        }

        construction.SetName($"[{prefix}{construction.Name} {GetPostfix()}]");
    }
    private string GetPostfix()
    {
        _globalRevision += Random.Range(1, 11);

        if (_globalRevision > 128)
        {
            _globalRevision = 0;

            _lettersIndex++;
        }
        string letterCode = GetCode(_lettersIndex);

        return $"{letterCode}-{_globalRevision}";
    }
    private string GetCode(int index)
    {
        string resultString = string.Empty;

        while (index >= 0)
        {
            resultString = (char)('A' + (index % 26)) + resultString;
            index = (index / 26) - 1;
        }

        return resultString;
    }
}
