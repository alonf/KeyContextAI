using System.Text;
using KeyContextAI.Core.Contracts;
using KeyContextAI.Core.Model;

namespace KeyContextAI.Core.Engines;

/// <inheritdoc cref="IWordAssemblyEngine" />
public sealed class WordAssemblyEngine : IWordAssemblyEngine
{
    private readonly StringBuilder _text = new();
    private readonly List<int> _scanCodes = [];

    /// <inheritdoc />
    public WordAssemblyResult Append(KeyEvent key)
    {
        ArgumentNullException.ThrowIfNull(key);

        // FR-013: our own injected keystrokes must never re-enter the pipeline, or a correction
        // could trigger a correction.
        if (key.IsSelfInjected)
        {
            return NoChange();
        }

        switch (key.Kind)
        {
            case KeyEventKind.Character when key.Character is { } ch:
                _text.Append(ch);
                _scanCodes.Add(key.ScanCode);
                return InProgress();

            case KeyEventKind.Editing:
                return Shorten();

            case KeyEventKind.Separator:
            case KeyEventKind.Committing:
                return Complete(key.Kind == KeyEventKind.Committing);

            case KeyEventKind.Modifier:
            case KeyEventKind.Character:
                // A modifier contributes nothing, and a character key that produced no character
                // (a dead key, for instance) leaves the word as it was.
                return NoChange();

            case KeyEventKind.Other:
            default:
                // The caret may have moved anywhere, so the accumulated word no longer describes
                // what is on screen and must not be offered for correction.
                Reset();
                return NoChange();
        }
    }

    /// <inheritdoc />
    public void Reset()
    {
        _text.Clear();
        _scanCodes.Clear();
    }

    private WordAssemblyResult Complete(bool byCommittingKey)
    {
        if (_text.Length == 0)
        {
            return NoChange();
        }

        var result = new WordAssemblyResult(
            WordAssemblyOutcome.WordCompleted,
            _text.ToString(),
            [.. _scanCodes],
            byCommittingKey);

        Reset();
        return result;
    }

    private WordAssemblyResult Shorten()
    {
        if (_text.Length == 0)
        {
            return NoChange();
        }

        _text.Length--;
        _scanCodes.RemoveAt(_scanCodes.Count - 1);
        return InProgress();
    }

    private WordAssemblyResult InProgress() =>
        new(WordAssemblyOutcome.WordInProgress, _text.ToString(), [.. _scanCodes], CompletedByCommittingKey: false);

    private WordAssemblyResult NoChange() =>
        new(WordAssemblyOutcome.NoChange, _text.ToString(), [.. _scanCodes], CompletedByCommittingKey: false);
}
