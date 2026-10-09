"""Embed current C# source and the offline walkthrough. Run with Python 3."""
import hashlib
import json
from pathlib import Path
from chronological import build_chronology

HERE = Path(__file__).resolve().parent
FILES = ["Program.cs", "Models/Project.cs", "Models/StatusTypes.cs"]

def build():
    sources = {}
    for name in FILES:
        text = (HERE.parent / "ProjectHealthTracker" / name).read_text(encoding="utf-8-sig")
        sources[name] = dict(text=text, sha256=hashlib.sha256(text.encode()).hexdigest())
    guide = json.loads((HERE / "lessons.json").read_text(encoding="utf-8"))
    guide["chronology"] = build_chronology(sources)
    payload = json.dumps(dict(sources=sources, guide=guide), ensure_ascii=False)
    payload = payload.replace("<", "\\u003c").replace("&", "\\u0026")
    page = (HERE / "template.html").read_text(encoding="utf-8")
    assert page.count("__GUIDE_DATA__") == 1
    page = page.replace("__GUIDE_DATA__", payload)
    css = (HERE / "guide.css").read_text(encoding="utf-8") + (HERE / "chronological.css").read_text(encoding="utf-8")
    page = page.replace("__GUIDE_CSS__", css).replace("__GUIDE_JS__", (HERE / "chronological.js").read_text(encoding="utf-8"))
    (HERE / "learn.html").write_text(page, encoding="utf-8", newline="\n")
    print(f"Built guide: {len(guide['chronology'])} blocks, {len(sources)} current source files.")

if __name__ == "__main__":
    build()
