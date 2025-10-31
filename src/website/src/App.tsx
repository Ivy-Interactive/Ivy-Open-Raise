import Hero from "./components/Hero/Hero";
import TextSection from "./components/TextSection/TextSection";
import Feature from "./components/Feature/Feature";
import Footer from "./components/Footer/Footer";

import "./App.css";

function App() {
  return (
    <div className="App">
      <Hero
        title={`Raise Capital\nwith Confidence`}
        description={`Connect with top-tier venture capital firms\nand accelerate your startup's growth.\nOur platform streamlines the fundraising process.`}
        h2Title="Success Stories"
        smallTitle={`Join thousands of founders who've raised over $10B`}
        primaryCtaText="Join the waitlist"
        secondaryCtaText="Book a meeting"
        onPrimaryCtaClick={() => {}}
        onSecondaryCtaClick={() => {}}
        ctaMediaType="video"
        ctaMediaSrc="https://www.youtube.com/watch?v=dQw4w9WgXcQ"
        ctaMediaAlt="Success Story"
      />
      <TextSection />
      <Feature />
      <Footer />
    </div>
  );
}

export default App;
