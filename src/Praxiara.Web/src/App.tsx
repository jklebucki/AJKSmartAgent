import { useTranslation } from 'react-i18next'
import { NavLink, Navigate, Route, Routes } from 'react-router-dom'
import { ApprovalPanel } from './features/approvals/ApprovalPanel'
import { BrowserPanel } from './features/browser-view/BrowserPanel'
import { ChatPanel } from './features/chat/ChatPanel'
import { TaskTimeline } from './features/task-timeline/TaskTimeline'
import { IfsEnvironmentPanel } from './features/ifs-environments/IfsEnvironmentPanel'
import { IfsEndpointManagementPage } from './features/ifs-environments/IfsEndpointManagementPage'
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

      <nav className="app-navigation" aria-label={t('navigation.label')}>
        <NavLink end to="/">{t('navigation.workspace')}</NavLink>
        <NavLink to="/ifs-endpoints">{t('navigation.ifsEndpoints')}</NavLink>
      </nav>
      <Routes>
        <Route path="/" element={(
          <main className="workspace" aria-label={t('workspace.label')}>
            <aside className="left-rail">
              <ChatPanel />
              <TaskTimeline />
              <IfsEnvironmentPanel />
            </aside>
            <BrowserPanel />
            <ApprovalPanel />
          </main>
        )} />
        <Route path="/ifs-endpoints" element={<IfsEndpointManagementPage />} />
        <Route path="*" element={<Navigate replace to="/" />} />
      </Routes>
    </div>
  )
}

export default App
