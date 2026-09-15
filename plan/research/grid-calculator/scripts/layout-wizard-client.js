
        // ── Constants & State ──
        const MM_PER_IN = 25.4;
        const PT_PER_MM = 2.834645;
        let currentUnit = 'mm'; 
        let hasCalculated = false;
        let isSpread = true;
        let lastResult = null;

        const WORKER_URL = 'https://layout-wizard-calculator.noisy-art-eee3.workers.dev';

        const FIELD_IDS  = ['docW','docH','lm','rm','cols','cg','tm','bm','rows','rg'];
        const LABEL_IDS  = ['lblDocW','lblDocH','lblLm','lblRm','lblCols','lblCg','lblTm','lblBm','lblRows','lblRg'];
        const UNITLESS   = [4, 8]; // cols, rows indices

        const LABEL_TEXT = {
            mm: ['Width (mm)','Height (mm)','Left margin (mm)','Right margin (mm)','Column count','Column gutter (mm)','Top margin (mm)','Bottom margin (mm)','Row count','Row gutter (mm)'],
            in: ['Width (in)','Height (in)','Left margin (in)','Right margin (in)','Column count','Column gutter (in)','Top margin (in)','Bottom margin (in)','Row count','Row gutter (in)'],
        };
        const PLACEHOLDERS = {
            mm: ['e.g. 210','e.g. 297','e.g. 20','e.g. 20','e.g. 4','e.g. 5','e.g. 20','e.g. 20','e.g. 6','e.g. 5'],
            in: ['e.g. 8.268','e.g. 11.693','e.g. 0.787','e.g. 0.787','e.g. 4','e.g. 0.197','e.g. 0.787','e.g. 0.787','e.g. 6','e.g. 0.197'],
        };

        // ── Helpers ──
        const toMm = (v) => currentUnit === 'in' ? Math.round(v * MM_PER_IN * 100000) / 100000 : v;
        const fmt  = (n) => (Math.round(n * 1000) / 1000).toString();

        function updateBaselineHint() {
            const el = document.getElementById('blHint');
            if (!el) return;
            const desired = parseFloat(document.getElementById('baseline').value);
            const docHmm  = toMm(parseFloat(document.getElementById('docH').value));
            if (!isNaN(desired) && desired > 0 && !isNaN(docHmm) && docHmm > 0) {
                const docHpt = Math.round(docHmm * PT_PER_MM * 1000) / 1000;
                const N      = Math.round(docHpt / desired);
                const applied = Math.round(docHpt / N * 1000) / 1000;
                if (applied === desired) {
                    el.textContent = `✓ exact — ${N} lines`;
                    el.style.color = 'var(--green)';
                } else {
                    el.textContent = `→ ${applied} pt (${N} lines in ${fmt(docHmm)} mm)`;
                    el.style.color = 'var(--blue)';
                }
            } else {
                el.textContent = (!isNaN(docHmm) && docHmm > 0) ? 'enter a value' : 'enter doc height first';
                el.style.color = 'var(--text-dim)';
            }
        }

        function setUnit(unit) {
            if (unit === currentUnit) return;
            FIELD_IDS.forEach((id, i) => {
                if (UNITLESS.includes(i)) return;
                const el = document.getElementById(id);
                const v = parseFloat(el.value);
                if (!isNaN(v)) {
                    const inMm = currentUnit === 'mm' ? v : v * MM_PER_IN;
                    el.value = unit === 'mm' ? fmt(inMm) : (Math.round(inMm / MM_PER_IN * 1000) / 1000);
                }
            });
            currentUnit = unit;
            document.getElementById('btnMm').classList.toggle('active', unit === 'mm');
            document.getElementById('btnIn').classList.toggle('active', unit === 'in');
            LABEL_IDS.forEach((id, i) => {
                const el = document.getElementById(id);
                if (!el) return;
                if (id === 'lblLm') el.textContent = isSpread ? `Inside (${unit})` : LABEL_TEXT[unit][2];
                else if (id === 'lblRm') el.textContent = isSpread ? `Outside (${unit})` : LABEL_TEXT[unit][3];
                else el.textContent = LABEL_TEXT[unit][i];
            });
            FIELD_IDS.forEach((id, i) => {
                if (!UNITLESS.includes(i)) document.getElementById(id).placeholder = PLACEHOLDERS[unit][i];
            });
            if (hasCalculated) calculate();
        }

        function setSpread(spread) {
            isSpread = spread;
            document.getElementById('btnSingle').classList.toggle('active', !spread);
            document.getElementById('btnSpread').classList.toggle('active', spread);
            const ul = currentUnit;
            document.getElementById('lblLm').textContent = spread ? `Inside (${ul})` : `Left margin (${ul})`;
            document.getElementById('lblRm').textContent = spread ? `Outside (${ul})` : `Right margin (${ul})`;
            if (hasCalculated) calculate();
        }

        async function calculate() {
            const errorBox = document.getElementById('errorBox');
            errorBox.className = 'error-box';
            errorBox.innerHTML = '';

            const payload = {
                width: parseFloat(document.getElementById('docW').value),
                height: parseFloat(document.getElementById('docH').value),
                inside: parseFloat(document.getElementById('lm').value),
                outside: parseFloat(document.getElementById('rm').value),
                top: parseFloat(document.getElementById('tm').value),
                bottom: parseFloat(document.getElementById('bm').value),
                columnCount: parseInt(document.getElementById('cols').value),
                columnGutter: parseFloat(document.getElementById('cg').value),
                rowCount: parseInt(document.getElementById('rows').value),
                rowGutter: parseFloat(document.getElementById('rg').value),
                baselineLeading: parseFloat(document.getElementById('baseline').value),
                isSpread: isSpread,
                unit: currentUnit
            };

            const errors = [];
            if (isNaN(payload.width) || isNaN(payload.height)) errors.push('Document dimensions are required.');
            if (isNaN(payload.baselineLeading) || payload.baselineLeading <= 0) errors.push('Baseline leading is required.');

            if (errors.length) {
                errorBox.textContent = errors.join(' ');
                errorBox.className = 'error-box show';
                return;
            }

            const btn = document.querySelector('.btn-calc');
            btn.textContent = 'Calculating...';
            btn.disabled = true;

            try {
                const response = await fetch(WORKER_URL, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });

                if (!response.ok) {
                    const errorDetails = await response.text();
                    throw new Error(errorDetails || 'Service Unreachable');
                }

                const result = await response.json();
                if (result.error) throw new Error(result.error);
                
                lastResult = result;
                const card = document.getElementById('resultCard');
                card.classList.add('show');
                
                // Scroll to result for mobile visibility
                card.scrollIntoView({ behavior: 'smooth', block: 'start' });
                
                // Wait for DOM reflow so clientWidth is accurate
                setTimeout(() => {
                    drawGrid(result.preview);
                }, 50);
                
                hasCalculated = true;

            } catch (e) {
                errorBox.textContent = e.message;
                errorBox.className = 'error-box show';
            } finally {
                btn.textContent = 'Calculate';
                btn.disabled = false;
            }
        }

        function drawGrid(p) {
            const canvas = document.getElementById('gridCanvas');
            const container = canvas.parentElement;
            if (!container || !p) return;

            const containerW = container.clientWidth;
            if (containerW <= 0) return; // Guard against hidden container
            
            const PAD = 40;
            const pages = p.isSpread ? 2 : 1;
            
            // Scaled based on height = 1.0. The docW in p is the aspect ratio.
            const totalWidthUnits = p.docW * pages;
            const S = (containerW - PAD * 2) / totalWidthUnits;
            const cw = containerW;
            const ch = Math.round(1.0 * S + PAD * 2);
            
            const dpr = window.devicePixelRatio || 1;
            canvas.width = cw * dpr;
            canvas.height = ch * dpr;
            canvas.style.width = cw + 'px';
            canvas.style.height = ch + 'px';
            
            const ctx = canvas.getContext('2d');
            ctx.scale(dpr, dpr);

            ctx.fillStyle = '#0d0d0d';
            ctx.fillRect(0, 0, cw, ch);

            const drawPage = (ox, oy, flipH) => {
                const ilm = flipH ? p.rm : p.lm;
                const irm = flipH ? p.lm : p.rm;

                const mTop    = oy + p.tm * S;
                const mBottom = oy + (p.docH - p.bm) * S;
                const mLeft   = ox + ilm * S;
                const mRight  = ox + (p.docW - irm) * S;

                // Paper + Shadow
                ctx.save();
                ctx.shadowColor = 'rgba(0,0,0,0.4)';
                ctx.shadowBlur = 12;
                ctx.shadowOffsetX = 4;
                ctx.shadowOffsetY = 4;
                ctx.fillStyle = '#ffffff';
                ctx.fillRect(ox, oy, p.docW * S, p.docH * S);
                ctx.restore();

                // Modular Document Grid
                ctx.beginPath();
                ctx.strokeStyle = 'rgba(0,0,0,0.15)'; // Darker as requested
                ctx.lineWidth = 0.5;
                const maxLines = 400;
                if ((p.docW / p.unitH) <= maxLines) {
                    for (let x = 0; x <= p.docW + 0.01; x += p.unitH) {
                        ctx.moveTo(ox + x * S, oy);
                        ctx.lineTo(ox + x * S, oy + p.docH * S);
                    }
                }
                if ((p.docH / p.unitV) <= maxLines) {
                    for (let y = 0; y <= p.docH + 0.01; y += p.unitV) {
                        ctx.moveTo(ox, oy + y * S);
                        ctx.lineTo(ox + p.docW * S, oy + y * S);
                    }
                }
                ctx.stroke();

                // Column tints (Purple-ish)
                ctx.fillStyle = 'rgba(141,118,201,0.08)';
                for (let c = 0; c < p.cols; c++) {
                    const cx = ox + (ilm + c * (p.colW + p.cg)) * S;
                    ctx.fillRect(cx, mTop, p.colW * S, mBottom - mTop);
                }

                // Margin guides (Magenta-ish)
                ctx.strokeStyle = 'rgba(230,0,100,0.85)';
                ctx.lineWidth = 0.75;
                ctx.strokeRect(mLeft, mTop, mRight - mLeft, mBottom - mTop);

                // Column guides (Purple-ish)
                ctx.strokeStyle = 'rgba(141,118,201,0.7)';
                ctx.lineWidth = 0.5;
                for (let c = 0; c < p.cols; c++) {
                    const vx = ox + (ilm + c * (p.colW + p.cg)) * S;
                    ctx.strokeRect(vx, mTop, p.colW * S, mBottom - mTop);
                }

                // Row guides (Blue-ish)
                ctx.strokeStyle = 'rgba(0,100,220,0.25)';
                ctx.lineWidth = 0.5;
                for (let r = 0; r < p.rows; r++) {
                    const ry = oy + (p.tm + r * (p.rowH + p.rg)) * S;
                    ctx.strokeRect(mLeft, ry, mRight - mLeft, p.rowH * S);
                }
            };

            if (p.isSpread) {
                drawPage(PAD, PAD, true);
                drawPage(PAD + p.docW * S, PAD, false);
                ctx.strokeStyle = 'rgba(0,0,0,0.5)';
                ctx.lineWidth = 1;
                ctx.beginPath();
                ctx.moveTo(PAD + p.docW * S, PAD);
                ctx.lineTo(PAD + p.docW * S, PAD + p.docH * S);
                ctx.stroke();
            } else {
                drawPage(PAD, PAD, false);
            }
        }

        function resetAll() {
            FIELD_IDS.forEach(id => {
                document.getElementById(id).value = '';
            });
            document.getElementById('baseline').value = '12';
            document.getElementById('errorBox').className = 'error-box';
            document.getElementById('resultCard').className = 'result-card';
            document.getElementById('blHint').textContent = '';
            hasCalculated = false;
            lastResult = null;
            updateBaselineHint();
        }

        function exportPreset() {
            if (!hasCalculated || !lastResult) return;
            try {
                let txt = atob(lastResult.presetBase64);
                
                // Read optional preset name
                const rawName = document.getElementById('presetName').value.trim();
                const now = new Date();
                const timestamp = now.getFullYear() +
                    ('0' + (now.getMonth()+1)).slice(-2) +
                    ('0' + now.getDate()).slice(-2) + '-' +
                    ('0' + now.getHours()).slice(-2) + 
                    ('0' + now.getMinutes()).slice(-2) + 
                    ('0' + now.getSeconds()).slice(-2);
                
                const finalName = rawName || timestamp;
                const safeFileName = finalName.replace(/[^a-z0-9_\-]/gi, '_');

                // Update Preset Name within the text file
                // We assume there's a line: 'Preset Name: Web Export;'
                txt = txt.replace(/Preset Name: [^;]+;/, `Preset Name: ${finalName};`);

                const blob = new Blob([txt], { type: 'text/plain' });
                const a = document.createElement('a');
                a.href = URL.createObjectURL(blob);
                a.download = `${safeFileName}.txt`;
                a.click();
                URL.revokeObjectURL(a.href);
            } catch (e) {
                console.error('Export failed');
            }
        }

        // Live hints
        document.getElementById('baseline').addEventListener('input', updateBaselineHint);
        document.getElementById('docH').addEventListener('input', updateBaselineHint);
        updateBaselineHint();
    