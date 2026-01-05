import { useEffect, useMemo, useState } from 'react';
import './App.css';

type PhaseSuccessRate = {
    phase: string;
    launchCount: number;
    successCount: number;
    failureCount: number;
    successRate: number;
};

type AnalysisResponse = {
    generatedAt: string;
    from: string;
    to: string;
    launchCount: number;
    fourPhase: PhaseSuccessRate[];
    eightPhase: PhaseSuccessRate[];
};

type ViewMode = 'four' | 'eight';

function App() {
    const [analysis, setAnalysis] = useState<AnalysisResponse | null>(null);
    const [viewMode, setViewMode] = useState<ViewMode>('four');
    const [isClosed, setIsClosed] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadAnalysis();
    }, []);

    const chartData = useMemo<PhaseSuccessRate[]>(() => {
        if (!analysis) return [];
        return viewMode === 'four' ? analysis.fourPhase : analysis.eightPhase;
    }, [analysis, viewMode]);

    const maxLaunches = useMemo(() => Math.max(...chartData.map((c) => c.launchCount), 1), [chartData]);

    async function loadAnalysis() {
        try {
            setIsLoading(true);
            setError(null);
            const response = await fetch('/api/analysis/success-rates');
            if (!response.ok) {
                throw new Error('Backend lieferte einen Fehler.');
            }
            const data = (await response.json()) as AnalysisResponse;
            setAnalysis(data);
        } catch (err) {
            const message = err instanceof Error ? err.message : 'Unbekannter Fehler beim Laden.';
            setError(message);
        } finally {
            setIsLoading(false);
        }
    }

    const handleExit = () => {
        setIsClosed(true);
        try {
            window.close();
        } catch (err) {
            // ignore - browser may block closing
        }
    };

    if (isClosed) {
        return (
            <div className="app">
                <header>
                    <h1>Rocket & Moon Dashboard</h1>
                    <p>Sitzung beendet. Aktualisiere die Seite, um erneut zu starten.</p>
                </header>
            </div>
        );
    }

    return (
        <div className="app">
            <header>
                <div>
                    <p className="eyebrow">Mondphasen × Raketenstarts</p>
                    <h1>Launch-Erfolgsraten nach Mondphase</h1>
                    <p className="lede">
                        Lade Launchdaten und Mondphasen von externen APIs, rechne sie aufbereitete Erfolgsraten um und stelle sie
                        dem Frontend bereit.
                    </p>
                </div>
                <div className="actions">
                    <button className="secondary" onClick={handleExit}>
                        Beenden
                    </button>
                    <button className="primary" onClick={loadAnalysis} disabled={isLoading}>
                        {isLoading ? 'Aktualisiere…' : 'Aktualisieren'}
                    </button>
                </div>
            </header>

            <section className="toolbar">
                <div className="selector">
                    <label htmlFor="view">Darstellung</label>
                    <div className="options">
                        <button
                            className={viewMode === 'four' ? 'chip active' : 'chip'}
                            onClick={() => setViewMode('four')}
                            aria-pressed={viewMode === 'four'}
                        >
                            4 Mondphasen
                        </button>
                        <button
                            className={viewMode === 'eight' ? 'chip active' : 'chip'}
                            onClick={() => setViewMode('eight')}
                            aria-pressed={viewMode === 'eight'}
                        >
                            8 Mondphasen
                        </button>
                    </div>
                </div>
                <div className="meta">
                    <div>
                        <p className="label">Zeitraum</p>
                        <p className="value">
                            {analysis
                                ? `${new Date(analysis.from).toLocaleDateString()} – ${new Date(analysis.to).toLocaleDateString()}`
                                : 'lädt…'}
                        </p>
                    </div>
                    <div>
                        <p className="label">Berücksichtigte Launches</p>
                        <p className="value">{analysis ? analysis.launchCount : '…'}</p>
                    </div>
                    <div>
                        <p className="label">Zuletzt aktualisiert</p>
                        <p className="value">{analysis ? new Date(analysis.generatedAt).toLocaleString() : '…'}</p>
                    </div>
                </div>
            </section>

            {error && <div className="alert">{error}</div>}

            <div className="panel">
                <div className="panel-header">
                    <div>
                        <p className="label">Analyse</p>
                        <h2>{viewMode === 'four' ? 'Erfolgsrate nach 4 Mondphasen' : 'Erfolgsrate nach 8 Mondphasen'}</h2>
                    </div>
                    <div className="legend">
                        <span className="pill success" /> Erfolgreich
                        <span className="pill failure" /> Fehlgeschlagen
                    </div>
                </div>

                <div className="chart-area">
                    {isLoading && <p>Analysedaten werden geladen…</p>}
                    {!isLoading && chartData.length === 0 && <p>Noch keine Daten vorhanden.</p>}
                    {!isLoading && chartData.length > 0 && (
                        <div className="bars" role="img" aria-label="Gestapelte Balken pro Mondphase">
                            {chartData.map((item) => {
                                const successHeight = Math.max((item.successCount / maxLaunches) * 100, 1);
                                const failureHeight = Math.max((item.failureCount / maxLaunches) * 100, 1);
                                return (
                                    <div className="bar-group" key={item.phase}>
                                        <div className="bar-stack" aria-label={`${item.phase}: ${item.launchCount} Launches`}>
                                            <div
                                                className="bar-segment success"
                                                style={{ height: `${successHeight}%` }}
                                                title={`Erfolgreich: ${item.successCount}`}
                                            />
                                            <div
                                                className="bar-segment failure"
                                                style={{ height: `${failureHeight}%` }}
                                                title={`Fehlgeschlagen: ${item.failureCount}`}
                                            />
                                        </div>
                                        <p className="bar-label">{item.phase}</p>
                                    </div>
                                );
                            })}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}

export default App;
