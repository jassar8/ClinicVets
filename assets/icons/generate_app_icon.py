"""
Build assets/icons/app.ico — multi-size ICO for Windows shell and WinForms.
Run from repo root: python assets/icons/generate_app_icon.py
"""
from __future__ import annotations

import math
from pathlib import Path

from PIL import Image, ImageDraw

# Match UiTheme.HeaderBlue (30, 95, 164)
BLUE = (30, 95, 164, 255)
BLUE_DARK = (24, 79, 132, 255)
WHITE = (255, 255, 255, 255)


def draw_icon(size: int) -> Image.Image:
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    pad = max(1, round(size * 0.0625))
    r = max(2, round(size * 0.156))
    # Slight inset gradient feel: outer ring darker
    draw.rounded_rectangle(
        [pad, pad, size - pad - 1, size - pad - 1],
        radius=r,
        fill=BLUE,
        outline=BLUE_DARK,
        width=max(1, size // 64),
    )

    cx, cy = size // 2, size // 2
    # Medical cross (equal arms)
    arm = size * 0.2
    thick = max(2, round(size * 0.11))
    half = thick // 2
    # Vertical bar
    draw.rounded_rectangle(
        [cx - half, int(cy - arm), cx + thick - half, int(cy + arm)],
        radius=max(1, thick // 3),
        fill=WHITE,
    )
    # Horizontal bar
    draw.rounded_rectangle(
        [int(cx - arm), cy - half, int(cx + arm), cy + thick - half],
        radius=max(1, thick // 3),
        fill=WHITE,
    )

    # Paw accent (readable from 32px+)
    if size >= 32:
        _draw_paw(draw, size, anchor_x=int(size * 0.62), anchor_y=int(size * 0.72))

    return img


def _draw_paw(draw: ImageDraw.ImageDraw, size: int, anchor_x: int, anchor_y: int) -> None:
    scale = size / 256.0
    pr = max(2, int(5 * scale))  # pad radius
    # Main pad (oval)
    mw, mh = int(18 * scale), int(14 * scale)
    draw.ellipse(
        [anchor_x - mw, anchor_y - mh, anchor_x + mw, anchor_y + mh],
        fill=WHITE,
    )
    # Four toe pads in an arc above
    toe_r = max(2, int(6 * scale))
    dist = int(14 * scale)
    angles = (-110, -55, -10, 35)

    for deg in angles:
        rad = math.radians(deg)
        tx = anchor_x + int(dist * math.cos(rad))
        ty = anchor_y - int(dist * 0.55 * math.sin(rad)) - int(10 * scale)
        draw.ellipse(
            [tx - toe_r, ty - toe_r, tx + toe_r, ty + toe_r],
            fill=WHITE,
        )


def main() -> None:
    out = Path(__file__).resolve().parent / "app.ico"
    sizes = [16, 24, 32, 48, 64, 128, 256]
    images = [draw_icon(s) for s in sizes]
    images[0].save(
        out,
        format="ICO",
        sizes=[(s, s) for s in sizes],
        append_images=images[1:],
    )
    print(f"Wrote {out}")


if __name__ == "__main__":
    main()
