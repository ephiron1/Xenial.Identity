﻿using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using MudBlazor;
using MudBlazor.Utilities;

using Spectre.Console;

namespace Xenial.Identity;

public partial class TagStringEdit : MudBaseInput<string>
{
    [Inject] private IScrollManager ScrollManager { get; set; } = default!;

    private bool dense;

    protected string Classname =>
        new CssBuilder("mud-select")
        .AddClass(Class)
        .Build();

    /// <summary>
    /// User class names for the popover, separated by space
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListAppearance)]
    public string PopoverClass { get; set; }

    /// <summary>
    /// Set the anchor origin point to determen where the popover will open from.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListAppearance)]
    public Origin AnchorOrigin { get; set; } = Origin.BottomCenter;

    /// <summary>
    /// Sets the transform origin point for the popover.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListAppearance)]
    public Origin TransformOrigin { get; set; } = Origin.TopCenter;

    /// <summary>
    /// Set the anchor origin point to determen where the popover will open from.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Use AnchorOrigin or TransformOrigin instead.", true)]
    [Parameter] public Direction Direction { get; set; } = Direction.Bottom;

    /// <summary>
    /// If true, the Autocomplete menu will open either before or after the input (left/right).
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Use AnchorOrigin or TransformOrigin instead.", true)]
    [Parameter] public bool OffsetX { get; set; }

    /// <summary>
    /// If true, the Autocomplete menu will open either before or after the input (stringop/bottom).
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Obsolete("Use AnchorOrigin or TransformOrigin instead.", true)]
    [Parameter] public bool OffsetY { get; set; }

    /// <summary>
    /// If true, compact vertical padding will be applied to all Autocomplete items.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListAppearance)]
    public bool Dense
    {
        get => dense;
        set => dense = value;
    }

    /// <summary>
    /// The Open Autocomplete Icon
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public string OpenIcon { get; set; } = Icons.Material.Filled.ArrowDropDown;

    /// <summary>
    /// The Close Autocomplete Icon
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Appearance)]
    public string CloseIcon { get; set; } = Icons.Material.Filled.ArrowDropUp;

    //internal event Action<HashSet<string>> SelectionChangedFromOutside;

    /// <summary>
    /// The maximum height of the Autocomplete when it is open.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListAppearance)]
    public int MaxHeight { get; set; } = 300;

    private Func<string, string> toStringFunc = x => x;

    /// <summary>
    /// Defines how values are displayed in the drop-down list
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListBehavior)]
    public Func<string, string> ToStringFunc
    {
        get => toStringFunc;
        set
        {
            if (toStringFunc == value)
            {
                return;
            }

            toStringFunc = value;
            Converter = new Converter<string>
            {
                SetFunc = toStringFunc ?? (x => x?.ToString() ?? ""),
            };
        }
    }

    /// <summary>
    /// The SearchFunc returns a list of items matching the typed text
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListBehavior)]
    public Func<string, Task<IEnumerable<string>>> SearchFunc { get; set; } = _ => Task.FromResult(Enumerable.Empty<string>());

    /// <summary>
    /// Maximum items to display, defaults to 10.
    /// A null value will display all items.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListBehavior)]
    public int? MaxItems { get; set; } = 10;

    /// <summary>
    /// Minimum characters to initiate a search
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public int MinCharacters { get; set; } = 0;

    /// <summary>
    /// Reset value if user deletes the text
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public bool ResetValueOnEmptyText { get; set; } = false;

    /// <summary>
    /// If true, clicking the text field will select (highlight) its contents.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public bool SelectOnClick { get; set; } = true;

    /// <summary>
    /// Debounce interval in milliseconds.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public int DebounceInterval { get; set; } = 100;

    /// <summary>
    /// Optional presentation template for unselected items
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListBehavior)]
    public RenderFragment<string> ItemTemplate { get; set; }

    /// <summary>
    /// Optional presentation template for the selected item
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListBehavior)]
    public RenderFragment<string> ItemSelectedTemplate { get; set; }

    /// <summary>
    /// Optional presentation template for disabled item
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListBehavior)]
    public RenderFragment<string> ItemDisabledTemplate { get; set; }

    /// <summary>
    /// Optional presentation template for when more items were returned from the Search function than the MaxItems limit
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListBehavior)]
    public RenderFragment MoreItemsTemplate { get; set; }

    /// <summary>
    /// Optional presentation template for when no items were returned from the Search function
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListBehavior)]
    public RenderFragment NoItemsTemplate { get; set; }

    /// <summary>
    /// Function to be invoked when checking whether an item should be disabled or not
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListBehavior)]
    public Func<string, bool> ItemDisabledFunc { get; set; } = _ => false;

    private bool isOpen;

    /// <summary>
    /// Returns the open state of the drop-down.
    /// </summary>
    public bool IsOpen
    {
        get => isOpen;
        // Note: the setter is protected because it was needed by a user who derived his own autocomplete from this class.
        // Note: setting IsOpen will not open or close it. Use ToggleMenu() for that. 
        protected set
        {
            if (value == isOpen)
            {
                return;
            }

            isOpen = value;

            IsOpenChanged.InvokeAsync(isOpen).AndForget();
        }
    }

    /// <summary>
    /// An event triggered when the state of IsOpen has changed
    /// </summary>
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

    /// <summary>
    /// If true, the currently selected item from the drop-down (if it is open) is selected.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.ListBehavior)]
    public bool SelectValueOnTab { get; set; } = false;

    /// <summary>
    /// Show clear button.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.FormComponent.Behavior)]
    public bool Clearable { get; set; } = false;

    /// <summary>
    /// Button click event for clear button. Called after text and value has been cleared.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnClearButtonClick { get; set; }

    private string CurrentIcon => !string.IsNullOrWhiteSpace(AdornmentIcon) ? AdornmentIcon : isOpen ? CloseIcon : OpenIcon;

    /// <summary>
    /// This boolean will keep track if the clear function is called too keep the set text function to be called.
    /// </summary>
    private bool isCleared;

    private MudInput<string> elementReference;

    [Parameter]
    public IList<string> Tags { get; set; } = Array.Empty<string>();

    [Parameter]
    public EventCallback<IList<string>> TagsChanged { get; set; }

    public TagStringEdit()
    {
        Adornment = Adornment.End;
        IconSize = Size.Medium;
    }

    public async Task SelectOption(string value)
    {
        if (!string.IsNullOrEmpty(value) && !Tags.Contains(value))
        {
            if (Tags is string[])
            {
                Tags = new List<string>(Tags)
                {
                    value
                }.Distinct().ToList();
            }
            else
            {
                Tags.Add(value);
            }
            await TagsChanged.InvokeAsync(Tags);
            value = "";
        }
        else
        {
            value = "";
        }

        await SetValueAsync(value);
        if (_items != null)
            _selectedListItemIndex = Array.IndexOf(_items, value);
        var optionText = GetItemString(value);
        if (!isCleared)
            await SetTextAsync(optionText, false);
        _timer?.Dispose();
        IsOpen = false;
        BeginValidate();
        if (!isCleared)
        {
            elementReference?.SetText(optionText);
        }
        elementReference?.FocusAsync().AndForget();
        StateHasChanged();
    }

    /// <summary>
    /// Toggle the menu (if not disabled or not readonly, and is opened).
    /// </summary>
    public async Task ToggleMenu()
    {
        if ((Disabled || ReadOnly) && !IsOpen)
        {
            return;
        }
        await ChangeMenu(!IsOpen);
    }

    private async Task ChangeMenu(bool open)
    {
        if (open)
        {
            if (SelectOnClick && elementReference is not null)
            {
                await elementReference.SelectAsync();
            }
            await OnSearchAsync();
        }
        else
        {
            _timer?.Dispose();
            RestoreScrollPosition();
            IsOpen = false;
            StateHasChanged();
        }
    }


    protected override void OnInitialized()
    {
        var text = GetItemString(Value);
        if (!string.IsNullOrWhiteSpace(text))
            Text = text;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        isCleared = false;
        base.OnAfterRender(firstRender);
    }

    private Timer _timer;
    private string[] _items;
    private int _selectedListItemIndex = 0;
    private IList<int> _enabledItemIndices = new List<int>();

    protected override Task UpdateTextPropertyAsync(bool updateValue)
    {
        _timer?.Dispose();
        // This keeps the text from being set when clear() was called
        if (isCleared)
            return Task.CompletedTask;
        return base.UpdateTextPropertyAsync(updateValue);
    }

    protected override async Task UpdateValuePropertyAsync(bool updateText)
    {
        _timer?.Dispose();
        if (ResetValueOnEmptyText && string.IsNullOrWhiteSpace(Text))
            await SetValueAsync("", updateText);
        if (DebounceInterval <= 0)
            await OnSearchAsync();
        else
            _timer = new Timer(OnTimerComplete, null, DebounceInterval, Timeout.Infinite);
    }

    private void OnTimerComplete(object stateInfo) => InvokeAsync(OnSearchAsync);

    private int _itemsReturned; //the number of items returned by the search function

    /// <remarks>
    /// This async method needs to return a task and be awaited in order for
    /// unit tests that trigger this method to work correctly.
    /// </remarks>
    private async Task OnSearchAsync()
    {
        if (MinCharacters > 0 && (string.IsNullOrWhiteSpace(Text) || Text.Length < MinCharacters))
        {
            IsOpen = false;
            StateHasChanged();
            return;
        }

        IEnumerable<string> searched_items = Array.Empty<string>();
        try
        {
            searched_items = (await SearchFunc(Text)) ?? Array.Empty<string>();
        }
        catch (Exception e)
        {
            AnsiConsole.WriteLine("The search function failed to return results: " + e.Message);
        }
        _itemsReturned = searched_items.Count();
        if (MaxItems.HasValue)
        {
            searched_items = searched_items.Take(MaxItems.Value);
        }
        _items = searched_items.ToArray();

        _enabledItemIndices = _items.Select((item, idx) => (item, idx)).Where(stringuple => ItemDisabledFunc?.Invoke(stringuple.item) != true).Select(tuple => tuple.idx).ToList();
        _selectedListItemIndex = _enabledItemIndices.Any() ? _enabledItemIndices.First() : -1;

        IsOpen = true;

        if (_items?.Length == 0)
        {
            StateHasChanged();
            return;
        }

        StateHasChanged();
    }

    private int _elementKey = 0;

    /// <summary>
    /// Clears the autocomplete's text
    /// </summary>
    public async Task Clear()
    {
        isCleared = true;
        IsOpen = false;
        await SetTextAsync(string.Empty, updateValue: false);
        if (elementReference != null)
            await elementReference.SetText("");
        _timer?.Dispose();
        StateHasChanged();
    }

    protected override async void ResetValue()
    {
        await Clear();
        base.ResetValue();
    }


    private string GetItemString(string item)
    {
        if (item == null)
            return string.Empty;
        try
        {
            return Converter.Set(item);
        }
        catch (NullReferenceException) { }
        return "null";
    }

    internal virtual async Task OnInputKeyDown(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "Tab":
                // NOTE: We need to catch Tab in Keydown because a tab will move focus to the next element and thus
                // in OnInputKeyUp we'd never get the tab key
                if (!IsOpen)
                    return;
                if (SelectValueOnTab)
                    await OnEnterKey();
                else
                    IsOpen = false;
                break;
        }
    }

    internal virtual async Task OnInputKeyUp(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "Enter":
            case "NumpadEnter":
                if (!IsOpen)
                {
                    await ToggleMenu();
                }
                else
                {
                    await OnEnterKey();
                }
                break;
            case "ArrowDown":
                if (!IsOpen)
                {
                    await ToggleMenu();
                }
                else
                {
                    var increment = _enabledItemIndices.ElementAtOrDefault(_enabledItemIndices.IndexOf(_selectedListItemIndex) + 1) - _selectedListItemIndex;
                    await SelectNextItem(increment < 0 ? 1 : increment);
                }
                break;
            case "ArrowUp":
                if (args.AltKey == true)
                {
                    await ChangeMenu(open: false);
                }
                else if (!IsOpen)
                {
                    await ToggleMenu();
                }
                else
                {
                    var decrement = _selectedListItemIndex - _enabledItemIndices.ElementAtOrDefault(_enabledItemIndices.IndexOf(_selectedListItemIndex) - 1);
                    await SelectNextItem(-(decrement < 0 ? 1 : decrement));
                }
                break;
            case "Escape":
                await ChangeMenu(open: false);
                break;
            case "Tab":
                await Task.Delay(1);
                if (!IsOpen)
                    return;
                if (SelectValueOnTab)
                    await OnEnterKey();
                else
                    await ToggleMenu();
                break;
            case "Backspace":
                if (args.CtrlKey == true && args.ShiftKey == true)
                {
                    if (string.IsNullOrEmpty(Text) && Tags.Count > 0)
                    {
                        Tags.Remove(Tags.Last());
                        await TagsChanged.InvokeAsync(Tags);
                    }
                    Reset();
                }
                break;
        }
        base.InvokeKeyUp(args);
    }

    private ValueTask SelectNextItem(int increment)
    {
        if (increment == 0 || _items == null || _items.Length == 0 || !_enabledItemIndices.Any())
            return ValueTask.CompletedTask;
        // if we are at the end, or the beginning we just do an rollover
        _selectedListItemIndex = Math.Clamp(value: (10 * _items.Length + _selectedListItemIndex + increment) % _items.Length, min: 0, max: _items.Length - 1);
        return ScrollToListItem(_selectedListItemIndex);
    }

    /// <summary>
    /// We need a random id for the year items in the year list so we can scroll to the item safely in every DatePicker.
    /// </summary>
    private readonly string _componentId = Guid.NewGuid().ToString();


    /// <summary>
    /// Scroll to a specific item index in the Autocomplete list of items.
    /// </summary>
    /// <param name="index">the index to scroll to</param>
    /// <param name="increment">not used</param>
    /// <returns>ValueTask</returns>
    [Obsolete("Use ScrollToListItem without increment parameter instead")]
    public Task ScrollToListItem(int index, int increment)
        => ScrollToListItem(index).AsTask();

    /// <summary>
    /// Scroll to a specific item index in the Autocomplete list of items.
    /// </summary>
    /// <param name="index">the index to scroll to</param>
    public ValueTask ScrollToListItem(int index)
    {
        var id = GetListItemId(index);
        //id of the scrolled element
        return ScrollManager.ScrollToListItemAsync(id);
    }

    //This restores the scroll position after closing the menu and element being 0
    private void RestoreScrollPosition()
    {
        if (_selectedListItemIndex != 0) return;
        ScrollManager.ScrollToListItemAsync(GetListItemId(0));
    }

    private string GetListItemId(in int index)
    {
        return $"{_componentId}_item{index}";
    }

    internal Task OnEnterKey()
    {
        if (IsOpen == false)
            return Task.CompletedTask;
        if (_items == null || _items.Length == 0)
            return Task.CompletedTask;
        if (_selectedListItemIndex >= 0 && _selectedListItemIndex < _items.Length)
            return SelectOption(_items[_selectedListItemIndex]);
        return Task.CompletedTask;
    }

    private Task OnInputBlurred(FocusEventArgs args)
    {
        OnBlur.InvokeAsync(args);
        return Task.CompletedTask;
        // we should not validate on blur in autocomplete, because the user needs to click out of the input to select a value,
        // resulting in a premature validation. thus, don't call base
        //base.OnBlurred(args);
    }

    protected override void Dispose(bool disposing)
    {
        _timer?.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// Focus the input in the Autocomplete component.
    /// </summary>
    public override ValueTask FocusAsync()
    {
        return elementReference.FocusAsync();
    }

    /// <summary>
    /// Blur from the input in the Autocomplete component.
    /// </summary>
    public override ValueTask BlurAsync()
    {
        return elementReference.BlurAsync();
    }

    /// <summary>
    /// Select all text within the Autocomplete input.
    /// </summary>
    public override ValueTask SelectAsync()
    {
        return elementReference.SelectAsync();
    }

    /// <summary>
    /// Select all text within the Autocomplete input and aligns its start and end points to the text content of the current input.
    /// </summary>
    public override ValueTask SelectRangeAsync(int pos1, int pos2)
    {
        return elementReference.SelectRangeAsync(pos1, pos2);
    }

    private async Task OnTextChanged(string text)
    {
        await base.TextChanged.InvokeAsync();

        if (text == null)
            return;
        await SetTextAsync(text, true);
    }

    private async Task ListItemOnClick(string item)
    {
        await SelectOption(item);
    }

    private void Closed(MudChip chip) => Tags.Remove(chip.Text);
}
