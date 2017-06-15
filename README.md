# XAMARIN ANDROID BINDING LIBRARY
    uCrop - Image Cropping Library for Android

<img src="https://raw.githubusercontent.com/Yalantis/uCrop/master/preview.gif" width="800" height="600" />

# Usage

*For a working implementation, please have a look at the Sample Project - sample*

<a href="https://play.google.com/store/apps/details?id=com.yalantis.ucrop.sample&utm_source=global_co&utm_medium=prtnr&utm_content=Mar2515&utm_campaign=PartBadge&pcampaignid=MKT-AC-global-none-all-co-pr-py-PartBadges-Oct1515-1"><img alt="Get it on Google Play" src="https://play.google.com/intl/en_us/badges/images/generic/en_badge_web_generic.png" width="185" height="70"/></a>

1. Include the library via nuget
```
Install-Package Naxam.uCrop.Droid
```

2. Add UCropActivity into your AndroidManifest.xml

    ```
    <activity
        android:name="com.yalantis.ucrop.UCropActivity"
        android:screenOrientation="portrait"
        android:theme="@style/Theme.AppCompat.Light.NoActionBar"/>
    ```

3. The uCrop configuration is created using the builder pattern.

	```java
    UCrop.Of(sourceUri, destinationUri)
        .WithAspectRatio(16, 9)
        .WithMaxResultSize(maxWidth, maxHeight)
        .Start(context);
    ```


4. Override `OnActivityResult` method and handle uCrop result.

    ```c#
    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data) {
        if (resultCode == Result.Ok && requestCode == UCrop.RequestCrop) {
            Uri resultUri = UCrop.GetOutput(data);
        } else if (resultCode == UCrop.ResultError) {
            Throwable cropError = UCrop.GetError(data);
        }
    }
    ```
5. You may want to add this to your PROGUARD config:

    ```
    -dontwarn com.yalantis.ucrop**
    -keep class com.yalantis.ucrop** { *; }
    -keep interface com.yalantis.ucrop** { *; }
    ```

# Customization

If you want to let your users choose crop ratio dynamically, just do not call `WithAspectRatio(x, y)`.

uCrop builder class has method `WithOptions(UCrop.Options options)` which extends library configurations.

Currently you can change:

   * image compression format (e.g. PNG, JPEG, WEBP), compression
   * image compression quality [0 - 100]. PNG which is lossless, will ignore the quality setting.
   * whether all gestures are enabled simultaneously
   * maximum size for Bitmap that is decoded from source Uri and used within crop view. If you want to override default behaviour.
   * toggle whether to show crop frame/guidelines
   * setup color/width/count of crop frame/rows/columns
   * choose whether you want rectangle or oval crop area
   * the UI colors (Toolbar, StatusBar, active widget state)
   * and more...
    
# Compatibility
  
  * Library - Android ICS 4.0+ (API 14) (Android GINGERBREAD 2.3+ (API 10) for versions <= 1.3.2)
  * Sample - Android ICS 4.0+ (API 14)
  * CPU - armeabi armeabi-v7a x86 x86_64 arm64-v8a (for versions >= 2.1.2)

## License
This binding is licensed under the MIT license, but the native Java library is license under the Apache License by Yalantis.

    Copyright 2017, Yalantis

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.