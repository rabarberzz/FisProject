namespace ControllerApp.Validation;

public class NumericValidatationBehavior : Behavior<Entry>
{
    protected override void OnAttachedTo(Entry bindable)
    {
        bindable.TextChanged += OnTextChanged;
        base.OnAttachedTo(bindable);
    }

    protected override void OnDetachingFrom(Entry bindable)
    {
        bindable.TextChanged -= OnTextChanged;
        base.OnDetachingFrom(bindable);
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = (Entry)sender;

        // Allow empty input or numeric input
        if (!string.IsNullOrEmpty(entry.Text) && !decimal.TryParse(entry.Text, out _))
        {
            // Revert to the previous valid value
            entry.Text = e.OldTextValue;
        }
    }
}