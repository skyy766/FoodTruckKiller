"""
FoodTruckKiller - Batch Unity .meta generator
Agent: A4 (pixel-artist)
Task : 为所有 PNG 与 WAV 生成 Unity 兼容的 .meta

约定：
  - PNG: TextureImporter  PPU=32, Point filter, AlphaIsTransparency, Single sprite, pivot=(0.5,0.5)
  - WAV: AudioImporter    DecompressOnLoad, PreloadAudioData, default settings

GUID 规则：基于文件路径生成稳定 MD5 前 32 hex（16 字节）。
"""

import os
import hashlib
import sys

PROJECT_ROOT = "/workspace/FoodTruckKiller"


def guid_for(path: str) -> str:
    """根据绝对路径生成稳定 GUID（Unity 16 字节 = 32 hex）。"""
    rel = os.path.relpath(path, PROJECT_ROOT).replace("\\", "/")
    h = hashlib.md5(rel.encode("utf-8")).hexdigest()
    return h


def write_meta_png(png_path: str):
    rel = os.path.relpath(png_path, PROJECT_ROOT).replace("\\", "/")
    guid = guid_for(png_path)
    # 32x32 像素，1 个 sprite
    # 默认 PPU=32
    body = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  mipmapLimitGroupName:
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 0
    mipBias: 0
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 32
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  alphaSource: 0
  ETC1ExternalAlphaChannel: 0
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    meta_path = png_path + ".meta"
    with open(meta_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(body)
    print(f"  meta: {rel}.meta")


def write_meta_wav(wav_path: str):
    rel = os.path.relpath(wav_path, PROJECT_ROOT).replace("\\", "/")
    guid = guid_for(wav_path)
    body = f"""fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 7
  defaultSettings:
    serializedVersion: 2
    loadType: 1
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 1
    quality: 0.5
    conversionMode: 0
  forceToMono: 0
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    meta_path = wav_path + ".meta"
    with open(meta_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(body)
    print(f"  meta: {rel}.meta")


def main():
    skip_dirs = {".git", "Library", "Temp", "obj", "Build", "Logs", "MemoryCaptures",
                 ".vs", ".idea", "node_modules"}
    png_count = wav_count = meta_skipped = 0
    for root, dirs, files in os.walk(PROJECT_ROOT):
        dirs[:] = [d for d in dirs if d not in skip_dirs]
        for f in files:
            full = os.path.join(root, f)
            if f.endswith(".meta"):
                continue
            ext = os.path.splitext(f)[1].lower()
            if ext == ".png":
                write_meta_png(full)
                png_count += 1
            elif ext == ".wav":
                write_meta_wav(full)
                wav_count += 1
    print(f"\nTotal: {png_count} PNG + {wav_count} WAV metas generated.")


if __name__ == "__main__":
    main()
