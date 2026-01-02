# wUBL
wasd_'s Ultimate Background Library for The Battle of Polytopia

This mod allows modders advanced theming of the main menu. Users can switch between the backgrounds of other mods using a lil button.

## For Users

You can switch between `menuData`s with the backgroundSwitch button, located at the top right corner of the main menu. That's it.

## For Modders

MAKE SURE TO TELL YOUR USERS TO USE wUBL IN ORDER TO MAKE THE MENU STUFF WORK!

You'll need 3 sprites:
* A Background sprite
* A Logo sprite
* An Icon sprite

Of these, none are required, so you have the ability to only change what you want to change. I highly suggest having an icon sprite.

Implementation in the `patch.json` file:

```json
{
    "*your usual patch.json data here*",

    "menuData": {
        "template": {
            "background": "wubletbg",
            "icon": "wubleticon",
            "title": "wublettitle",
            "scrollerGradientColor": "0,0,0,1"
        },
        "myOtherMenuDataThatOnlyChangesTheLogo": {
            "title": "wublettitle"
        }
    }
}
```

You can have multiple `menuData`s per mod (as seen above).

You need to write your sprites's names without underscores, but the files have to have underscores regardless.

`"scrollerGradientColor"` refers to the color of the ScrollerGradient found in the tribe selection screen using an RGBA code. Normally it's pink, but that clashes with some background color schemes, so I added a way to recolour it.

*NOTE: ScrollerGradientColor is not finished, as I have yet to figure out why it's tinted pink even when I recolour it in code. For now, setting it to `0,0,0,1` switches it to black, and setting it to `0,0,0,0` makes it transparent. Otherwise it's supposed to be a regular RGBA value*


