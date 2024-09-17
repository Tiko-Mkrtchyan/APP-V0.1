using System;

public interface IMediaLoader
{
    void Setup<T>(T media);
    void SetSelected(bool selected);
    bool IsSelected { get; }
    event Action OnLoaded;
}