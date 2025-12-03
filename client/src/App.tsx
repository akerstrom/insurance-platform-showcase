import { CustomerSearch } from './components/CustomerSearch';

function App() {
  return (
    <div>
      <header>
        <div>
          <div>
            <a href="/">
              <div>
                <span>ThreadPilot</span>
              </div>
            </a>
          </div>
        </div>
      </header>

      <main>
        <CustomerSearch />
      </main>
    </div>
  );
}

export default App;
