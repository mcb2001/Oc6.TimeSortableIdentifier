# Oc6 TimeSortableIdentifier

A time sortable unique identifier with 64 bits given as a `System.Int64 (long)`

up to 255 (byte.MaxValue) is guaranteed to be sortable every millisecond.

## Create

```
long newRandomTimeSortableIdentifier = Tsid.Create();
```

## TryParse

```
string text = "0123-4567-89AB-CDEF";

if(Tsid.TryParse(text, out long tsid))
{
    //Use the tsid
}
else
{
    //Handle parse error
}
```

## ToString

```
long tsid = 81985529216486895L;

string text = Tsid.ToString(value);

//Outputs 0123-4567-89AB-CDEF
Console.WriteLine(text);
```
