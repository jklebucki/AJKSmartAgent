import { useTranslation } from 'react-i18next'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { NavLink, Navigate, Route, Routes, useLocation } from 'react-router-dom'
import { ApprovalPanel } from './features/approvals/ApprovalPanel'
import { BrowserPanel } from './features/browser-view/BrowserPanel'
import { ChatPanel } from './features/chat/ChatPanel'
import { TaskTimeline } from './features/task-timeline/TaskTimeline'
import { IfsEnvironmentPanel } from './features/ifs-environments/IfsEnvironmentPanel'
import { IfsEndpointManagementPage } from './features/ifs-environments/IfsEndpointManagementPage'
import { getAuthenticationSession, logout } from './shared/api/ifsEnvironments'
import './App.css'

export function App() {
  const { t } = useTranslation()
  const location = useLocation()
  const queryClient = useQueryClient()
  const sessionQuery = useQuery({ queryKey: ['authentication-session'], queryFn: getAuthenticationSession })
  const logoutMutation = useMutation({
    mutationFn: logout,
    onSuccess: () => void queryClient.setQueryData(['authentication-session'], null),
  })
  const loginUrl = `/api/v1/auth/login?returnUrl=${encodeURIComponent(location.pathname)}`

  return (
    <div className="app-shell">
      <header className="app-header">
        <div>
          <span className="eyebrow">{t('product.category')}</span>
          <h1>Praxiara</h1>
        </div>
        <div className="authentication-controls">
          {sessionQuery.data === null && <a className="secondary-button" href={loginUrl}>{t('authentication.login')}</a>}
          {sessionQuery.data !== null && sessionQuery.data !== undefined && <>
            <div className="environment-badge" aria-label={t('authentication.session')}>
              <span className="status-dot is-connected" aria-hidden="true" />
              {t('authentication.signedInAs', { userName: sessionQuery.data.userName })}
            </div>
            <button className="secondary-button" type="button" disabled={logoutMutation.isPending} onClick={() => void logoutMutation.mutateAsync()}>{t('authentication.logout')}</button>
          </>}
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
