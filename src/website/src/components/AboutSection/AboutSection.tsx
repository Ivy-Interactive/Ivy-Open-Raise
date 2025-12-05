import React from "react";
import styles from "./AboutSection.module.scss";
import { MessageSquare, ClipboardList, Mail, Clock } from "lucide-react";

const painPoints = [
  {
    icon: <MessageSquare size={20} />,
    text: "Investor conversations get scattered across emails, DMs, spreadsheets, and memory.",
  },
  {
    icon: <ClipboardList size={20} />,
    text: "CRMs built for sales don't capture investor signals.",
  },
  {
    icon: <Mail size={20} />,
    text: "Decks are sent with no visibility into who actually opened them.",
  },
  {
    icon: <Clock size={20} />,
    text: "Deck tools track views but ignore relationships.",
  },
];

const AboutSection: React.FC = () => {
  return (
    <section className={styles.aboutSection} id="about">
      <div className={styles.container}>
        <div className={styles.badge}>
          <span className={styles.badgeText}>WHY TRADITIONAL TOOLS FAIL</span>
        </div>

        <h2 className={styles.title}>
          Fundraising is a relationship workflow, not a sales funnel.
        </h2>

        <div className={styles.painPoints}>
          {painPoints.map((point, index) => (
            <div key={index} className={styles.painPoint}>
              <span className={styles.icon}>{point.icon}</span>
              <p className={styles.pointText}>{point.text}</p>
            </div>
          ))}
        </div>

        <div className={styles.divider} />

        <blockquote className={styles.quote}>
          "Founders lose momentum not because they lack activity, but because
          context slips away."
        </blockquote>
      </div>
    </section>
  );
};

export default AboutSection;
