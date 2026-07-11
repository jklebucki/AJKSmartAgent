import { useTranslation } from 'react-i18next'
import { ApprovalPanel } from './features/approvals/ApprovalPanel'
import { BrowserPanel } from './features/browser-view/BrowserPanel'
import { ChatPanel } from './features/chat/ChatPanel'
import { TaskTimeline } from './features/task-timeline/TaskTimeline'
import './App.css'

export function App() {
  const { t } = useTranslation()

  return (
    <div className="app-shell">
      <header className="app-header">
        <div>
          <span className="eyebrow">{t('product.category')}</span>
          <h1>Praxiara</h1>
        </div>
        <div className="environment-badge" aria-label={t('environment.label')}>
          <span className="status-dot" aria-hidden="true" />
          {t('environment.notConnected')}
        </div>
      </header>

      <main className="workspace" aria-label={t('workspace.label')}>
        <aside className="left-rail">
          <ChatPanel />
          <TaskTimeline />
        </aside>
        <BrowserPanel />
        <ApprovalPanel />
      </main>
    </div>
  )
}

export default App
