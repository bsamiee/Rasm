# Quick Start Tutorial

Get up and running with Grid Calculator PE in just a few minutes. This guide will walk you through creating your first grid calculation.

## Launch Grid Calculator PE

1. **Open the application**
   - Double-click the Grid Calculator PE icon
   - Wait for the application to load
   - You'll see the main calculator interface

2. **Familiarize yourself with the interface**
   - Main calculation area (center)
   - Presets panel (left sidebar)
   - Results panel (right sidebar)
   - Tools menu (top bar)

## Your First Grid Calculation

Let's create a simple 12-column grid for a 1200px wide container:

### Step 1: Set Container Width
1. In the **Container Width** field, enter `1200`
2. Select `px` from the unit dropdown
3. The calculator will update automatically

### Step 2: Choose Grid Settings
1. Set **Number of Columns** to `12`
2. Set **Gutter Width** to `20px`
3. Choose **Margin Type**: `Equal margins`
4. Set **Side Margins** to `40px`

### Step 3: View Results
Your grid calculation will show:
- **Column Width**: 80px
- **Total Gutter Space**: 220px (11 gutters × 20px)
- **Content Area**: 1120px (1200px - 80px margins)

## Common Grid Types

### Responsive Web Grid
Perfect for modern websites:

| Setting | Value |
|---------|-------|
| Container | 1200px |
| Columns | 12 |
| Gutters | 20px |
| Margins | 5% |

### Print Layout Grid
For magazine or book design:

| Setting | Value |
|---------|-------|
| Container | 8.5in |
| Columns | 3 |
| Gutters | 0.25in |
| Margins | 1in |

### Mobile Grid
For smartphone layouts:

| Setting | Value |
|---------|-------|
| Container | 375px |
| Columns | 4 |
| Gutters | 16px |
| Margins | 20px |

## Export Your Grid

Once you're happy with your calculations:

1. Click the **Export** button
2. Choose your format:
   - **CSS Code** - Ready-to-use CSS grid
   - **Design Specs** - PDF with measurements
   - **JSON Data** - Raw calculation data

### Sample CSS Output
```css
.grid-container {
  width: 1200px;
  display: grid;
  grid-template-columns: repeat(12, 80px);
  gap: 20px;
  padding: 0 40px;
  margin: 0 auto;
}
```

## Using Presets

Save time with built-in presets:

1. Click **Presets** in the left panel
2. Choose from common grid systems:
   - Bootstrap 12-column
   - CSS Grid Foundation
   - Print Magazine Standard
   - Mobile First Responsive

3. Click any preset to instantly apply those settings

## Pro Tips

### Golden Ratio Grids
- Click **Tools** → **Golden Ratio**
- Perfect for harmonious proportions
- Great for editorial design

### Unit Conversion
- Switch between px, %, em, rem, inches, mm
- All calculations update automatically
- Perfect for cross-platform design

### Responsive Preview
- Use **View** → **Responsive Preview**
- See how your grid adapts to different screen sizes
- Test breakpoints visually

## Troubleshooting

### Grid doesn't fit?
- Check if gutters + margins exceed container width
- Reduce gutter size or number of columns
- Use percentage-based measurements for flexibility

### Weird decimal numbers?
- Switch to a different unit system
- Use **Tools** → **Round Numbers** for cleaner values
- Consider adjusting container width slightly

## Next Steps

Now that you know the basics:

- **[Explore basic features](basic-features.md)** - Learn all the calculator tools
- **[Master advanced features](advanced-features.md)** - Discover professional techniques  
- **[Check out pro features](pro-features.md)** - See what makes PE special

## Need Help?

- **[Common Issues](common-issues.md)** - Quick solutions
- **[FAQ](faq.md)** - Frequently asked questions
- **[Contact Support](support.md)** - Get personalized help

---

*Congratulations! You've created your first grid with Grid Calculator PE. Happy designing!*