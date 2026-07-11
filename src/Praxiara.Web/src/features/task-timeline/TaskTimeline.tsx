import { useTranslation } from 'react-i18next'

export function TaskTimeline() {
  const { t } = useTranslation()

  return (
    <section className="panel timeline-panel" aria-labelledby="timeline-title">
      <h2 id="timeline-title">{t('timeline.title')}</h2>
      <ol className="timeline">
        <li>
          <span className="timeline-marker" aria-hidden="true" />
          {t('timeline.created')}
        </li>
      </ol>
    </section>
  )
}
