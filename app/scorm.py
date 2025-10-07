"""Utilities for SCORM package inspection."""
from __future__ import annotations

import posixpath
import zipfile
from dataclasses import dataclass
from typing import Iterable
from xml.etree import ElementTree as ET


class ManifestNotFoundError(FileNotFoundError):
    """Raised when a SCORM manifest cannot be located in an archive."""


class ManifestParseError(ValueError):
    """Raised when a SCORM manifest cannot be parsed."""


@dataclass(slots=True)
class ManifestDetails:
    entry_point: str
    version: str
    manifest_path: str
    manifest_xml: str


IGNORED_DIRS = {"__MACOSX"}


def _normalise_path(path: str) -> str:
    cleaned = posixpath.normpath(path)
    if cleaned.startswith("../") or cleaned.startswith("..\\"):
        raise ManifestParseError("Manifest references parent directories, which is not allowed")
    return cleaned


def read_manifest(archive: zipfile.ZipFile) -> ManifestDetails:
    """Parse imsmanifest.xml from a zip archive."""

    manifest_name = None
    for name in archive.namelist():
        if name.endswith("/"):
            continue
        if any(part in IGNORED_DIRS for part in name.split("/")):
            continue
        if name.lower().endswith("imsmanifest.xml"):
            manifest_name = name
            break
    if manifest_name is None:
        raise ManifestNotFoundError("imsmanifest.xml not found in SCORM package")

    manifest_bytes = archive.read(manifest_name)
    try:
        root = ET.fromstring(manifest_bytes)
    except ET.ParseError as exc:
        raise ManifestParseError("Unable to parse imsmanifest.xml") from exc

    entry_point = _extract_entry_point(root)
    if not entry_point:
        raise ManifestParseError("Unable to determine entry point from manifest")

    version = _extract_version(root)
    manifest_dir = posixpath.dirname(manifest_name)
    entry_path = _normalise_path(posixpath.join(manifest_dir, entry_point)) if manifest_dir else _normalise_path(entry_point)

    return ManifestDetails(
        entry_point=entry_path,
        version=version,
        manifest_path=manifest_name,
        manifest_xml=manifest_bytes.decode("utf-8", errors="replace"),
    )


def _iter_elements(root: ET.Element, tag: str) -> Iterable[ET.Element]:
    suffix = tag.lower()
    for element in root.iter():
        if element.tag.lower().endswith(suffix):
            yield element


def _extract_entry_point(root: ET.Element) -> str | None:
    identifier_ref = None
    for item in _iter_elements(root, "item"):
        identifier_ref = item.attrib.get("identifierref")
        if identifier_ref:
            break
    if not identifier_ref:
        return None

    for resource in _iter_elements(root, "resource"):
        if resource.attrib.get("identifier") == identifier_ref:
            href = resource.attrib.get("href")
            if href:
                return href
            for file_node in _iter_elements(resource, "file"):
                href = file_node.attrib.get("href")
                if href:
                    return href
    return None


def _extract_version(root: ET.Element) -> str:
    for tag in ("schemaversion", "adlcp:schemaversion"):
        node = next(_iter_elements(root, tag), None)
        if node is not None and node.text:
            return node.text.strip()

    version = root.attrib.get("version")
    if version:
        return version.strip()
    return "SCORM 1.2"


__all__ = [
    "ManifestDetails",
    "ManifestNotFoundError",
    "ManifestParseError",
    "read_manifest",
]
