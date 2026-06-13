using System.Collections.Generic;

public interface IBuildable
{
    public string Name { get; }
    public void SetName(string newName);
    public void Initialize(List<RendererData> data);
}