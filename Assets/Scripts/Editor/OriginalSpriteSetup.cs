using System.IO;
using UnityEditor;
using UnityEngine;

public static class OriginalSpriteSetup
{
    public static void Extract()
    {
        var map = new Texture2D(2, 2);
        map.LoadImage(File.ReadAllBytes("Assets/SpritesJuego/NES - Super Mario Bros. - Stages - World 1-1.png"));
        Save(map, "Castle", 202 * 16, 128, 80, 80);
        Save(map, "FlagPole", 198 * 16, 32, 16, 176);
        Save(map, "Block_Hard", 80 * 16, 80, 16, 16);
        Directory.CreateDirectory("Assets/Sprites/BonusRoom");
        for(int y=0;y<13;y++) for(int x=0;x<16;x++)
        {
            bool solid=y<2 || (x==0 && y>=2) || (y==12 && x>=4 && x<=10) ||
                (x>=4 && x<=10 && y>=2 && y<=4) || x==15 || (x>=13 && y>=2 && y<=3);
            bool coin=((y==5||y==7)&&x>=4&&x<=10)||(y==9&&x>=5&&x<=9);
            if(solid||coin) Save(map,"BonusRoom/Tile_"+x+"_"+y,768+x*16,272+(12-y)*16,16,16);
        }
        Object.DestroyImmediate(map);
        ExtractNewSheets();
        AssetDatabase.Refresh();
        foreach (string name in new[] { "Castle", "FlagPole" })
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Sprites/" + name + ".png");
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(.5f, 0);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }

    private static void ExtractNewSheets()
    {
        var objects = new Texture2D(2,2);
        objects.LoadImage(File.ReadAllBytes("Assets/SpritesJuego/NES - Super Mario Bros. - Miscellaneous - Items, Objects and NPCs.png"));
        Save(objects,"LiftPlatform",116,64,48,8);
        Save(objects,"Item_Mushroom",0,8,16,16);
        Save(objects,"Item_Starman",106,8,16,16);
        Save(objects,"Item_Coin",180,36,16,16);
        Object.DestroyImmediate(objects);
        var sheet = new Texture2D(2,2);
        sheet.LoadImage(File.ReadAllBytes("Assets/SpritesJuego/NES - Super Mario Bros. - Miscellaneous - Title Screen, HUD and Miscellaneous.png"));
        Directory.CreateDirectory("Assets/Resources/ClassicUI");
        SaveScreen(sheet,"Title",0,192,new Color32(146,144,255,255),true);
        SaveScreen(sheet,"LevelIntro",0,460,Color.black,false);
        SaveStatus(sheet,"TimeUp",264);
        SaveStatus(sheet,"GameOver",528);
        var font = new Texture2D(MarioBros.UI.NesText.Characters.Length*8,8);
        for (int i=0;i<MarioBros.UI.NesText.Characters.Length;i++)
        {
            int column=i<24?i:i-24;int top=i<24?104:113;
            var glyph=sheet.GetPixels(column*9,sheet.height-top-8,8,8);
            for (int p=0;p<glyph.Length;p++)
                if (glyph[p].r < .9f || glyph[p].g < .9f || glyph[p].b < .9f) glyph[p]=Color.clear;
            font.SetPixels(i*8,0,8,8,glyph);
        }
        font.Apply();File.WriteAllBytes("Assets/Resources/ClassicUI/Font.png",font.EncodeToPNG());Object.DestroyImmediate(font);
        var coin=new Texture2D(8,8);coin.SetPixels(sheet.GetPixels(0,sheet.height-156-8,8,8));
        var coinPixels=coin.GetPixels();for(int i=0;i<coinPixels.Length;i++)
            if(IsSheetBackground(coinPixels[i])) coinPixels[i]=Color.clear;
        coin.SetPixels(coinPixels);coin.Apply();File.WriteAllBytes("Assets/Resources/ClassicUI/Coin.png",coin.EncodeToPNG());Object.DestroyImmediate(coin);
        var cursor=new Texture2D(8,8);var cp=sheet.GetPixels(0,sheet.height-440-8,8,8);
        for(int i=0;i<cp.Length;i++) if(IsSheetBackground(cp[i])) cp[i]=Color.clear;
        cursor.SetPixels(cp);cursor.Apply();File.WriteAllBytes("Assets/Resources/ClassicUI/Cursor.png",cursor.EncodeToPNG());Object.DestroyImmediate(cursor);
        Object.DestroyImmediate(sheet);
        AssetDatabase.Refresh();
        foreach (string name in new[] {"Title","LevelIntro","TimeUp","GameOver","Font","Coin","Cursor"})
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/ClassicUI/"+name+".png");
            importer.textureType = TextureImporterType.Sprite;importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;importer.mipmapEnabled = false;
            importer.npotScale=TextureImporterNPOTScale.None;importer.alphaIsTransparency=true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        ExtractPoses();
    }
    private static bool IsSheetBackground(Color color)
    {
        Color32 c=color;return (c.r==146&&c.g==144&&c.b==255)||(c.r==0&&c.g==41&&c.b==140);
    }

    private static void SaveStatus(Texture2D sheet,string name,int x)
    {
        var texture=new Texture2D(256,240);texture.SetPixels(sheet.GetPixels(x,sheet.height-460-240,256,240));
        ClearRect(texture,0,0,256,32,Color.black);ClearRect(texture,96,108,72,12,Color.black);
        texture.Apply();File.WriteAllBytes("Assets/Resources/ClassicUI/"+name+".png",texture.EncodeToPNG());Object.DestroyImmediate(texture);
    }
    private static void ExtractPoses()
    {
        var sheet=new Texture2D(2,2);sheet.LoadImage(File.ReadAllBytes("Assets/SpritesJuego/NES - Super Mario Bros. - Playable Characters - Mario & Luigi.png"));
        foreach (string name in new[] {"Big_Crouch","Big_Climb","Small_Climb"})
        {
            bool big=name.StartsWith("Big");int x=name=="Big_Crouch"?116:136;int y=big?32:8;int height=big?32:16;
            var texture=new Texture2D(16,height);var pixels=sheet.GetPixels(x,sheet.height-y-height,16,height);
            for(int i=0;i<pixels.Length;i++) { Color32 c=pixels[i];if((c.r==146&&c.g==144&&c.b==255)||(c.r==0&&c.g==41&&c.b==140)) pixels[i]=Color.clear; }
            texture.SetPixels(pixels);texture.Apply();string path="Assets/Resources/MarioFrames/"+name+".png";
            File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=16;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=9;settings.spritePivot=new Vector2(.5f,big?.25f:.5f);importer.SetTextureSettings(settings);importer.SaveAndReimport();
        }
        Object.DestroyImmediate(sheet);
    }

    private static void SaveScreen(Texture2D sheet,string name,int x,int y,Color background,bool title)
    {
        var texture = new Texture2D(256,240);
        texture.SetPixels(sheet.GetPixels(x,sheet.height-y-240,256,240));
        // Remove the sheet's colored variable placeholders; draw live values in the UI.
        ClearRect(texture,0,0,256,32,background);
        if (title)
        {
            ClearRect(texture,72,144,8,8,background);ClearRect(texture,72,160,8,8,background);
            ClearRect(texture,128,184,48,8,background);
        }
        else { ClearRect(texture,80,76,96,20,background);ClearRect(texture,134,108,24,16,background); }
        texture.Apply();File.WriteAllBytes("Assets/Resources/ClassicUI/"+name+".png",texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
    }
    private static void ClearRect(Texture2D texture,int x,int top,int width,int height,Color color)
    {
        for (int row=top;row<top+height;row++)
            for (int col=x;col<x+width;col++) texture.SetPixel(col,239-row,color);
    }

    private static void Save(Texture2D source, string name, int x, int top, int width, int height)
    {
        var pixels = source.GetPixels(x, source.height - top - height, width, height);
        Color32 sky = new Color32(92,148,252,255);
        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 c = pixels[i];
            if ((c.r == sky.r && c.g == sky.g && c.b == sky.b) ||
                (c.r == 146 && c.g == 144 && c.b == 255) || (c.r == 0 && c.g == 41 && c.b == 140)) pixels[i] = Color.clear;
        }
        var texture = new Texture2D(width, height);
        texture.SetPixels(pixels);texture.Apply();
        File.WriteAllBytes("Assets/Sprites/" + name + ".png", texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
    }
}
