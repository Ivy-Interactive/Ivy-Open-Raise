import Navbar from "./components/Navbar/Navbar";
import Hero from "./components/Hero/Hero";
import AboutSection from "./components/AboutSection/AboutSection";
import OpenSourceSection from "./components/OpenSourceSection/OpenSourceSection";
import ProductFeatures from "./components/ProductFeatures/ProductFeatures";
import Feature from "./components/Feature/Feature";
import HowToStartSection from "./components/HowToStartSection/HowToStartSection";
import CTASection from "./components/CTASection/CTASection";
import Footer from "./components/Footer/Footer";

import "./App.css";

function App() {
  return (
    <div className="App">
      <Navbar />
      <Hero
        badge="Explore Our New AI Features"
        title={`Smarter Fundraising for\nFaster Startup Growth`}
        description="Track your fundraising performance in real-time, manage investor pipelines effortlessly, and gain insights that help you close deals faster."
        primaryCtaText="View on GitHub"
        secondaryCtaText="Book a Demo"
        githubUrl="https://github.com/Ivy-Interactive/Ivy-Open-Raise"
      />
      <AboutSection />
      <OpenSourceSection />
      <ProductFeatures />
      <Feature />
      <HowToStartSection />
      <CTASection />
      <Footer />
    </div>
  );
}

export default App;
